using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Options;
using Recruiter.Application.ElevenLabs.Interfaces;
using Recruiter.Application.Interview.Interfaces;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Webhooks typically don't use Bearer auth - they use their own validation
public class WebhookController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookController> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly AzureStorageOptions _storageOptions;
    private readonly ITranscriptPathResolver _transcriptPathResolver;

    private const string FallbackWebhookFolder = "elevenlabs-webhooks";

    public WebhookController(
        IHttpClientFactory httpClientFactory,
        IOptions<PythonApiOptions> options,
        IFileStorageService fileStorageService,
        AzureStorageOptions storageOptions,
        ITranscriptPathResolver transcriptPathResolver,
        ILogger<WebhookController> logger)
    {
        _httpClient = httpClientFactory.CreateClient("PythonWebhookProxy");
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        _fileStorageService = fileStorageService;
        _storageOptions = storageOptions;
        _transcriptPathResolver = transcriptPathResolver;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ProxyWebhook(
        [FromServices] IElevenLabsWebhookValidator validator)
    {
        Request.EnableBuffering();
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        Request.Body.Position = 0;

        var signatureHeader = Request.Headers["elevenlabs-signature"].FirstOrDefault();
        var validationResult = validator.ValidateSignature(signatureHeader, payload);

        if (!validationResult.IsValid)
            return Unauthorized(new { error = validationResult.ErrorMessage });

        // Parse payload to extract conversation_id
        string conversationId = "";
        string transcriptJson = "";
        try
        {
            using var doc = JsonDocument.Parse(payload);

            // Check webhook type after validation
            if (doc.RootElement.TryGetProperty("type", out var typeProperty))
            {
                var webhookType = typeProperty.GetString();
                if (webhookType != "post_call_transcription")
                {
                    return Ok(new { status = "rejected" });
                }
            }

            // Navigate to data object
            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                // Extract conversation_id
                if (data.TryGetProperty("conversation_id", out var convId))
                {
                    conversationId = convId.GetString() ?? "";
                }

                // Extract transcript
                if (data.TryGetProperty("transcript", out var transcript))
                {
                    transcriptJson = transcript.GetRawText();
                }
            }

            // Validate extracted values
            if (string.IsNullOrEmpty(conversationId))
            {
                return BadRequest(new { error = "conversation_id is missing in payload" });
            }

            // Validate transcript presence
            if (string.IsNullOrEmpty(transcriptJson))
            {
                return BadRequest(new { error = "transcript is missing in payload" });
            }
        }
        catch
        {
            return BadRequest(new { error = "Invalid JSON payload" });
        }

        try
        {
            // 1. Resolve blob path using TranscriptPathResolver
            string blobPath;
            Guid? jobApplicationId = null;
            string? jobPostName = null;
            int? jobPostVersion = null;

            var pathResult = await _transcriptPathResolver.ResolvePathAsync(conversationId);
            if (pathResult is not null)
            {
                // Use job-application-specific path
                blobPath = pathResult.BlobPath;
                jobApplicationId = pathResult.JobApplicationId;
                jobPostName = pathResult.JobPostName;
                jobPostVersion = pathResult.JobPostVersion;

                _logger.LogInformation(
                    "Resolved transcript path for conversation {ConversationId}: {BlobPath}",
                    conversationId, blobPath);
            }
            else
            {
                return BadRequest(new { error = "No matching interview found for conversation_id" });
            }


            // 2. Store transcript in Azure Blob Storage
            var containerName = _storageOptions.ContainerName;
            using var payloadStream = new MemoryStream(Encoding.UTF8.GetBytes(transcriptJson));
            await _fileStorageService.UploadAsync(containerName, blobPath, payloadStream, "application/json");

            _logger.LogInformation(
                "Stored ElevenLabs webhook payload. ConversationId: {ConversationId}, BlobPath: {BlobPath}",
                conversationId, blobPath);

            // 3. Create request to Python with file reference
            var fileReference = new
            {
                container = containerName,
                blob_path = blobPath,
                conversation_id = conversationId,
                job_application_id = jobApplicationId,
                job_post_name = jobPostName,
                job_post_version = jobPostVersion,
                storage_account = _storageOptions.StorageAccountName
            };

            var targetUri = new Uri(_httpClient.BaseAddress!, "/api/webhook");
            var proxyRequest = new HttpRequestMessage(HttpMethod.Post, targetUri);

            // Copy relevant headers (excluding host and content-related)
            foreach (var header in Request.Headers)
            {
                if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) &&
                    !header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                {
                    proxyRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            // Send file reference as JSON body
            proxyRequest.Content = new StringContent(
                JsonSerializer.Serialize(fileReference),
                Encoding.UTF8,
                "application/json");

            // 4. Forward to Python server
            var response = await _httpClient.SendAsync(proxyRequest);
            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Python server. Is it running?");
            return StatusCode(502, new { error = "Cannot reach Python server", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook");
            return StatusCode(500, new { error = "Failed to process webhook request" });
        }
    }
}

