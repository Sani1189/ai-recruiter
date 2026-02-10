using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.ElevenLabs;
using Recruiter.Application.ElevenLabs.Dto;
using Recruiter.Application.ElevenLabs.Interfaces;

namespace Recruiter.Infrastructure.Services;

public class ElevenLabsService : IElevenLabsService
{
    private const string RateLimitCachePrefix = "ElevenLabsRate:";
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true
    };
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DictionaryKeyPolicy = null
    };

    private readonly HttpClient _httpClient;
    private readonly ElevenLabsOptions _options;
    private readonly ILogger<ElevenLabsService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMemoryCache _memoryCache;
    private readonly IElevenLabsConversationPayloadBuilder _payloadBuilder;

    public ElevenLabsService(
        HttpClient httpClient,
        IOptions<ElevenLabsOptions> options,
        ILogger<ElevenLabsService> logger,
        ICurrentUserService currentUserService,
        IMemoryCache memoryCache,
        IElevenLabsConversationPayloadBuilder payloadBuilder)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _payloadBuilder = payloadBuilder ?? throw new ArgumentNullException(nameof(payloadBuilder));
    }

    public async Task<Result<ConversationTokenResponseDto>> CreateConversationTokenAsync(
        ConversationTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsTokenRequestConfigured())
        {
            _logger.LogError("ElevenLabs agent configuration is missing. Cannot issue conversation token.");
            return MissingAgentConfiguration<ConversationTokenResponseDto>();
        }

        var agentId = string.IsNullOrWhiteSpace(request.AgentId) ? _options.AgentId : request.AgentId!;
        if (string.IsNullOrWhiteSpace(agentId))
        {
            return Result<ConversationTokenResponseDto>.Invalid(new ValidationError
            {
                ErrorMessage = "AgentId is required to request a conversation token."
            });
        }

        if (!TryConsumeRateLimit("token"))
        {
            _logger.LogWarning("ElevenLabs token request throttled for current user.");
            return Result<ConversationTokenResponseDto>.Error();
        }

        // Build session payload for frontend to use, but don't send it in token request
        Dictionary<string, object?>? sessionPayload = null;
        if (RequiresConversationPayload(request))
        {
            var payloadResult = await _payloadBuilder.BuildPayloadAsync(request, cancellationToken);
            if (!payloadResult.IsSuccess)
            {
                return payloadResult.Status switch
                {
                    ResultStatus.Invalid => Result<ConversationTokenResponseDto>.Invalid(payloadResult.ValidationErrors),
                    ResultStatus.NotFound => Result<ConversationTokenResponseDto>.NotFound(),
                    _ => Result<ConversationTokenResponseDto>.Error()
                };
            }

            sessionPayload = payloadResult.Value;
        }

        // Token endpoint is simple GET - no payload
        var response = await ExecuteWithRetryAsync(
            () => BuildTokenRequest(agentId, null),
            cancellationToken);

        if (!response.IsSuccess)
        {
            return Result<ConversationTokenResponseDto>.Error();
        }

        using var httpResponse = response.Value!;
        if (!httpResponse.IsSuccessStatusCode)
        {
            return await HandleErrorResponse<ConversationTokenResponseDto>(httpResponse, "requesting conversation token");
        }

        var tokenDto = await ParseTokenResponseAsync(httpResponse, agentId, sessionPayload, cancellationToken);
        return tokenDto;
    }

    public async Task<Result<ConversationDetailsDto>> GetConversationAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return Result<ConversationDetailsDto>.Invalid(new ValidationError
            {
                ErrorMessage = "ConversationId is required."
            });
        }

        if (!IsApiKeyConfigured())
        {
            _logger.LogError("ElevenLabs API key is not configured. Cannot retrieve conversation {ConversationId}.", conversationId);
            return MissingApiKey<ConversationDetailsDto>();
        }

        if (!TryConsumeRateLimit("conversation"))
        {
            _logger.LogWarning("ElevenLabs conversation lookup throttled for current user.");
            return Result<ConversationDetailsDto>.Error();
        }

        var endpoint = BuildConversationEndpoint(conversationId);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);

        HttpResponseMessage responseMessage;
        try
        {
            responseMessage = await SendAsync(httpRequest, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("ElevenLabs conversation lookup timed out for {ConversationId}.", conversationId);
            return Result<ConversationDetailsDto>.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving conversation details {ConversationId}.", conversationId);
            return Result<ConversationDetailsDto>.Error();
        }

        using var response = responseMessage;
        if (!response.IsSuccessStatusCode)
        {
            return await HandleErrorResponse<ConversationDetailsDto>(response, $"retrieving conversation '{conversationId}'");
        }

        var payloadDocument = await DeserializeAsync(response, cancellationToken);
        if (payloadDocument is not JsonDocument payload)
        {
            _logger.LogWarning("Unable to parse ElevenLabs conversation response for {ConversationId}.", conversationId);
            return Result<ConversationDetailsDto>.Error();
        }
        using (payload)
        {
            var root = payload.RootElement;
            
            // Log available properties to help debug
            var availableProperties = string.Join(", ", root.EnumerateObject().Select(p => p.Name));
            _logger.LogInformation("ElevenLabs conversation response for {ConversationId} contains properties: {Properties}", 
                conversationId, availableProperties);
            
            var transcript = TryGetTranscript(root);
            
            if (transcript == null || transcript.Count == 0)
            {
                // Log full response as JSON for debugging (only if transcript not found)
                var responseJson = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                _logger.LogWarning("No transcript found in ElevenLabs response for {ConversationId}. Full response: {Response}", 
                    conversationId, responseJson);
            }
            else
            {
                _logger.LogInformation("Successfully extracted {Count} transcript entries for conversation {ConversationId}", 
                    transcript.Count, conversationId);
            }
            
            var dto = new ConversationDetailsDto
            {
                ConversationId = conversationId,
                ConversationUrl = BuildAbsoluteUri(_options.ConversationEndpoint, conversationId),
                AudioUrl = BuildAbsoluteUri(_options.ConversationEndpoint, $"{conversationId}/audio"),
                Status = TryGetString(root, "status"),
                CreatedAt = TryGetDateTimeOffset(root, "created_at")
                            ?? TryGetUnixTimestamp(root, "created_at_unix"),
                Transcript = transcript,
            };

            return Result<ConversationDetailsDto>.Success(dto);
        }
    }

    public async Task<Result<ConversationAudioContent>> GetConversationAudioAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return Result<ConversationAudioContent>.Invalid(new ValidationError
            {
                ErrorMessage = "ConversationId is required."
            });
        }

        if (!IsApiKeyConfigured())
        {
            _logger.LogError("ElevenLabs API key is not configured. Cannot download audio for {ConversationId}.", conversationId);
            return MissingApiKey<ConversationAudioContent>();
        }

        if (!TryConsumeRateLimit("audio"))
        {
            _logger.LogWarning("ElevenLabs audio download throttled for current user.");
            return Result<ConversationAudioContent>.Error();
        }

        var endpoint = BuildConversationEndpoint($"{conversationId}/audio");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        httpRequest.Headers.Accept.ParseAdd(_options.AudioContentType);

        HttpResponseMessage responseMessage;
        try
        {
            responseMessage = await SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("ElevenLabs audio download timed out for {ConversationId}.", conversationId);
            return Result<ConversationAudioContent>.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving ElevenLabs audio {ConversationId}.", conversationId);
            return Result<ConversationAudioContent>.Error();
        }

        using var response = responseMessage;
        if (!response.IsSuccessStatusCode)
        {
            return await HandleErrorResponse<ConversationAudioContent>(response, $"retrieving conversation audio '{conversationId}'");
        }

        var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? _options.AudioContentType;

        if (data.Length == 0)
        {
            _logger.LogWarning("ElevenLabs returned empty audio content for conversation {ConversationId}", conversationId);
            return Result<ConversationAudioContent>.Error();
        }

        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                       ?? response.Content.Headers.ContentDisposition?.FileName;

        var dto = new ConversationAudioContent
        {
            Data = data,
            ContentType = contentType,
            FileName = fileName
        };

        return Result<ConversationAudioContent>.Success(dto);
    }

    private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        HttpCompletionOption completionOption,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(request, completionOption, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "ElevenLabs request timed out for {Method} {Uri}", request.Method, request.RequestUri);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling ElevenLabs for {Method} {Uri}", request.Method, request.RequestUri);
            throw;
        }
    }

    private async Task<Result<T>> HandleErrorResponse<T>(HttpResponseMessage response, string operation)
    {
        var status = (int)response.StatusCode;
        var body = await response.Content.ReadAsStringAsync();

        var message = $"ElevenLabs responded with HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) when {operation}.";
        _logger.LogWarning("ElevenLabs error {Status} during {Operation}. Payload: {Payload}", status, operation, body);

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized
                => Result<T>.Unauthorized(),
            HttpStatusCode.Forbidden
                => Result<T>.Forbidden(),
            HttpStatusCode.NotFound
                => Result<T>.NotFound(),
            HttpStatusCode.BadRequest
                => Result<T>.Invalid(new ValidationError { ErrorMessage = message }),
            _ => Result<T>.Error()
        };
    }

    private bool TryConsumeRateLimit(string scope)
    {
        if (_options.MaxRequestsPerMinute <= 0)
        {
            return true;
        }

        var window = TimeSpan.FromMinutes(1);
        var userKey = _currentUserService.GetUserId()
                       ?? _currentUserService.GetUserEmail()
                       ?? "anonymous";
        var cacheKey = $"{RateLimitCachePrefix}{scope}:{userKey}";

        var counter = _memoryCache.GetOrCreate(cacheKey, entry =>
        {
            entry.SetSlidingExpiration(window);
            return new SlidingWindowCounter();
        })!;

        var allowed = counter.TryConsume(_options.MaxRequestsPerMinute, window);
        if (allowed)
        {
            _memoryCache.Set(cacheKey, counter, new MemoryCacheEntryOptions
            {
                SlidingExpiration = window
            });
        }

        return allowed;
    }

    private string BuildTokenEndpoint(string agentId)
    {
        var endpoint = _options.TokenEndpoint.TrimStart('/');
        return $"{endpoint}?agent_id={Uri.EscapeDataString(agentId)}";
    }

    private HttpRequestMessage BuildTokenRequest(string agentId, Dictionary<string, object?>? payload)
    {
        if (payload is { Count: > 0 })
        {
            var requestPayload = new Dictionary<string, object?>(payload)
            {
                ["agent_id"] = agentId
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint.TrimStart('/'));
            var json = JsonSerializer.Serialize(requestPayload, SerializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return request;
        }

        return new HttpRequestMessage(HttpMethod.Get, BuildTokenEndpoint(agentId));
    }

    private static bool RequiresConversationPayload(ConversationTokenRequestDto request)
    {
        if (request is null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(request.InterviewConfigurationName)
               || !string.IsNullOrWhiteSpace(request.JobPostName)
               || request.JobApplicationId.HasValue;
    }

    private string BuildConversationEndpoint(string suffix)
    {
        var endpoint = _options.ConversationEndpoint.TrimStart('/');
        return $"{endpoint.TrimEnd('/')}/{suffix.TrimStart('/')}";
    }

    private Uri BuildAbsoluteUri(string basePath, string suffix)
    {
        var baseUrl = new Uri(_options.BaseUrl.EndsWith('/')
            ? _options.BaseUrl
            : _options.BaseUrl + "/");

        return new Uri(baseUrl, $"{basePath.TrimStart('/').TrimEnd('/')}/{suffix.TrimStart('/')}");
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? TryGetNestedString(JsonElement element, params string[] propertyPath)
    {
        if (propertyPath.Length == 0)
        {
            return null;
        }

        var current = element;
        foreach (var propertyName in propertyPath)
        {
            if (!current.TryGetProperty(propertyName, out var next))
            {
                return null;
            }

            current = next;
        }

        return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
    }

    private static DateTimeOffset? TryGetDateTimeOffset(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static DateTimeOffset? TryGetUnixTimestamp(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var unix))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix);
        }

        return null;
    }

    private static List<TranscriptEntryDto>? TryGetTranscript(JsonElement root)
    {
        // Try to find transcript array in multiple possible locations
        List<JsonElement>? transcriptArray = null;
        
        if (root.TryGetProperty("transcript", out var transcriptProp) && transcriptProp.ValueKind == JsonValueKind.Array)
        {
            transcriptArray = new List<JsonElement>();
            foreach (var item in transcriptProp.EnumerateArray())
            {
                transcriptArray.Add(item);
            }
        }
        else if (root.TryGetProperty("transcripts", out var transcriptsProp) && transcriptsProp.ValueKind == JsonValueKind.Array)
        {
            transcriptArray = new List<JsonElement>();
            foreach (var item in transcriptsProp.EnumerateArray())
            {
                transcriptArray.Add(item);
            }
        }
        else if (root.TryGetProperty("messages", out var messagesProp) && messagesProp.ValueKind == JsonValueKind.Array)
        {
            transcriptArray = new List<JsonElement>();
            foreach (var item in messagesProp.EnumerateArray())
            {
                transcriptArray.Add(item);
            }
        }

        if (transcriptArray == null || transcriptArray.Count == 0)
            return null;

        var transcript = new List<TranscriptEntryDto>();
        foreach (var entry in transcriptArray)
        {
            if (entry.ValueKind != JsonValueKind.Object)
                continue;

            // ElevenLabs API uses "message" field for the text content
            var messageText = TryGetString(entry, "message") 
                           ?? TryGetString(entry, "text") 
                           ?? TryGetString(entry, "content");

            // Extract role/source (usually "user" or "agent"/"assistant")
            var role = TryGetString(entry, "role") 
                    ?? TryGetString(entry, "source") 
                    ?? TryGetString(entry, "speaker");

            if (string.IsNullOrWhiteSpace(messageText))
                continue;

            var transcriptEntry = new TranscriptEntryDto
            {
                Role = role,
                Source = role,
                Speaker = role,
                Text = messageText,
                Content = messageText,
                Message = messageText,
            };

            transcript.Add(transcriptEntry);
        }

        return transcript.Count > 0 ? transcript : null;
    }

    private static List<JsonElement>? TryGetArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return null;

        if (value.ValueKind != JsonValueKind.Array)
            return null;

        var list = new List<JsonElement>();
        foreach (var item in value.EnumerateArray())
        {
            list.Add(item);
        }

        return list.Count > 0 ? list : null;
    }

    private static List<JsonElement>? TryGetNestedArray(JsonElement element, params string[] propertyPath)
    {
        if (propertyPath.Length == 0)
            return null;

        var current = element;
        for (var i = 0; i < propertyPath.Length - 1; i++)
        {
            if (!current.TryGetProperty(propertyPath[i], out var next) || next.ValueKind != JsonValueKind.Object)
                return null;

            current = next;
        }

        return TryGetArray(current, propertyPath[propertyPath.Length - 1]);
    }

    private static async Task<JsonDocument?> DeserializeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        if (stream.CanSeek && stream.Length == 0)
        {
            return null;
        }

        if (stream == Stream.Null)
        {
            return null;
        }

        try
        {
            return await JsonDocument.ParseAsync(stream, DocumentOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private bool IsApiKeyConfigured() => !string.IsNullOrWhiteSpace(_options.ApiKey);

    private bool IsTokenRequestConfigured() =>
        IsApiKeyConfigured() && !string.IsNullOrWhiteSpace(_options.AgentId);

    private Result<T> MissingApiKey<T>() =>
        Result<T>.Invalid(new ValidationError
        {
            ErrorMessage = "ElevenLabs API key is not configured. Update appsettings or secrets to enable this feature."
        });

    private Result<T> MissingAgentConfiguration<T>() =>
        Result<T>.Invalid(new ValidationError
        {
            ErrorMessage = "ElevenLabs agent identifier is not configured. Update appsettings or secrets to enable this feature."
        });

    private async Task<Result<HttpResponseMessage>> ExecuteWithRetryAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        const int baseDelayMs = 500;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var request = requestFactory();
                var response = await SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return Result<HttpResponseMessage>.Success(response);
                }

                // Retry on transient errors
                if (IsTransientError(response.StatusCode) && attempt < maxRetries)
                {
                    _logger.LogWarning(
                        "ElevenLabs returned {StatusCode} on attempt {Attempt}/{MaxRetries}, retrying...",
                        (int)response.StatusCode, attempt, maxRetries);
                    
                    response.Dispose();
                    await Task.Delay(baseDelayMs * attempt, cancellationToken);
                    continue;
                }

                return Result<HttpResponseMessage>.Success(response);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                lastException = ex;
                _logger.LogWarning(ex, "ElevenLabs request timed out on attempt {Attempt}/{MaxRetries}", 
                    attempt, maxRetries);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(baseDelayMs * attempt, cancellationToken);
                }
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "ElevenLabs request failed on attempt {Attempt}/{MaxRetries}", 
                    attempt, maxRetries);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(baseDelayMs * attempt, cancellationToken);
                }
            }
        }

        _logger.LogError(lastException, "ElevenLabs request failed after {MaxRetries} attempts", maxRetries);
        return Result<HttpResponseMessage>.Error();
    }

    private static bool IsTransientError(HttpStatusCode statusCode) => 
        statusCode >= HttpStatusCode.InternalServerError;

    private async Task<Result<ConversationTokenResponseDto>> ParseTokenResponseAsync(
        HttpResponseMessage response,
        string agentId,
        Dictionary<string, object?>? sessionPayload,
        CancellationToken cancellationToken)
    {
        var document = await DeserializeAsync(response, cancellationToken);
        if (document is not JsonDocument payload)
        {
            _logger.LogWarning("Unable to parse ElevenLabs token response");
            return Result<ConversationTokenResponseDto>.Error();
        }

        using (payload)
        {
            var root = payload.RootElement;
            var token = TryGetString(root, "webrtc_token")
                        ?? TryGetString(root, "token")
                        ?? TryGetString(root, "ws_token");

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("ElevenLabs returned empty token for agent {AgentId}", agentId);
                return Result<ConversationTokenResponseDto>.Error();
            }

            var dto = new ConversationTokenResponseDto
            {
                Token = token,
                ConversationId = TryGetString(root, "conversation_id")
                                ?? TryGetString(root, "conversationId")
                                ?? TryGetNestedString(root, "conversation", "id"),
                ExpiresAt = TryGetDateTimeOffset(root, "valid_until")
                            ?? TryGetDateTimeOffset(root, "expires_at")
                            ?? TryGetUnixTimestamp(root, "expires_at_unix"),
                AgentId = agentId,
                SessionPayload = sessionPayload != null ? JsonSerializer.Serialize(sessionPayload, SerializerOptions) : null
            };

            return Result<ConversationTokenResponseDto>.Success(dto);
        }
    }

    private sealed class SlidingWindowCounter
    {
        private readonly Queue<DateTimeOffset> _timestamps = new();
        private readonly object _lock = new();

        public bool TryConsume(int limit, TimeSpan window)
        {
            lock (_lock)
            {
                var now = DateTimeOffset.UtcNow;

                while (_timestamps.Count > 0 && now - _timestamps.Peek() > window)
                {
                    _timestamps.Dequeue();
                }

                if (_timestamps.Count >= limit)
                {
                    return false;
                }

                _timestamps.Enqueue(now);
                return true;
            }
        }
    }
}

