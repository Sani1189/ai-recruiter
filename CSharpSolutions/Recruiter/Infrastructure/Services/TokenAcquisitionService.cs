using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Options;

namespace Recruiter.Infrastructure.Services;

/// <summary>
/// Service for acquiring access tokens using OAuth 2.0 client credentials flow
/// </summary>
public class TokenAcquisitionService : ITokenAcquisitionService
{
    private readonly PythonApiOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenAcquisitionService> _logger;
    private readonly IConfidentialClientApplication _app;

    private const string CacheKey = "PythonApi_AccessToken";
    private const int TokenRefreshBufferMinutes = 5; // Refresh token 5 minutes before expiry

    public TokenAcquisitionService(
        IOptions<PythonApiOptions> options,
        IMemoryCache cache,
        ILogger<TokenAcquisitionService> logger)
    {
        _options = options.Value;
        _cache = cache;
        _logger = logger;

        // Validate configuration
        if (string.IsNullOrWhiteSpace(_options.ClientId))
            throw new InvalidOperationException("PythonApi:ClientId is required");
        if (string.IsNullOrWhiteSpace(_options.ClientSecret))
            throw new InvalidOperationException("PythonApi:ClientSecret is required");
        if (string.IsNullOrWhiteSpace(_options.TenantId))
            throw new InvalidOperationException("PythonApi:TenantId is required");
        if (string.IsNullOrWhiteSpace(_options.Scope))
            throw new InvalidOperationException("PythonApi:Scope is required");

        // Create MSAL confidential client application
        _app = ConfidentialClientApplicationBuilder
            .Create(_options.ClientId)
            .WithClientSecret(_options.ClientSecret)
            .WithAuthority(_options.Authority)
            .Build();
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_cache.TryGetValue(CacheKey, out CachedToken? cachedToken) && cachedToken != null)
        {
            // Check if token is still valid (with buffer for refresh)
            var expiresAt = cachedToken.ExpiresAt.AddMinutes(-TokenRefreshBufferMinutes);
            if (DateTime.UtcNow < expiresAt)
            {
                _logger.LogDebug("Returning cached access token");
                return cachedToken.Token;
            }

            _logger.LogDebug("Cached token expired or near expiry, acquiring new token");
        }

        try
        {
            _logger.LogInformation("Acquiring new access token for Python API");

            // Acquire token using client credentials flow
            var result = await _app
                .AcquireTokenForClient([_options.Scope])
                .ExecuteAsync(cancellationToken);

            // Cache the token
            var expirationTime = result.ExpiresOn.DateTime.ToUniversalTime();
            var cacheExpiration = expirationTime.AddMinutes(-TokenRefreshBufferMinutes);
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = cacheExpiration
            };

            var tokenToCache = new CachedToken
            {
                Token = result.AccessToken,
                ExpiresAt = expirationTime
            };

            _cache.Set(CacheKey, tokenToCache, cacheEntryOptions);
            _logger.LogInformation("Token: {Token}", result.AccessToken);
            _logger.LogInformation("Successfully acquired and cached access token. Expires at: {ExpiresAt}", expirationTime);
            return result.AccessToken;
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "Failed to acquire access token. Error: {ErrorCode}, Message: {Message}", ex.ErrorCode, ex.Message);
            throw new InvalidOperationException($"Failed to acquire access token: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while acquiring access token");
            throw;
        }
    }

    private class CachedToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

