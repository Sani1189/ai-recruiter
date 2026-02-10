namespace Recruiter.Application.Common.Interfaces;

/// <summary>
/// Service for acquiring access tokens for service-to-service authentication
/// </summary>
public interface ITokenAcquisitionService
{
    /// <summary>
    /// Gets an access token for the Python API using client credentials flow.
    /// Tokens are cached and automatically refreshed when expired.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token string</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

