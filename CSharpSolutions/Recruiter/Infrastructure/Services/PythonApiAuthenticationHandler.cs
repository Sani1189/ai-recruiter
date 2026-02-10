using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Options;

namespace Recruiter.Infrastructure.Services;

/// <summary>
/// HTTP message handler that automatically adds Bearer token and Function Key to requests to Python API
/// </summary>
public class PythonApiAuthenticationHandler : DelegatingHandler
{
    private readonly ITokenAcquisitionService _tokenService;
    private readonly PythonApiOptions _options;
    private readonly ILogger _logger;

    public PythonApiAuthenticationHandler(
        ITokenAcquisitionService tokenService,
        IOptions<PythonApiOptions> options,
        ILoggerFactory loggerFactory)
    {
        _tokenService = tokenService;
        _options = options.Value;
        _logger = loggerFactory.CreateLogger<PythonApiAuthenticationHandler>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get access token
            var token = await _tokenService.GetAccessTokenAsync(cancellationToken);

            // Add Bearer token to Authorization header
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Add Azure Function App function key as header if configured
            if (!string.IsNullOrWhiteSpace(_options.FunctionKey))
            {
                request.Headers.Add("x-functions-key", _options.FunctionKey);
                _logger.LogDebug("Added Bearer token and Function Key header to request: {RequestUri}", request.RequestUri);
            }
            else
            {
                _logger.LogDebug("Added Bearer token to request: {RequestUri}", request.RequestUri);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire token for request: {RequestUri}", request.RequestUri);
            throw;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

