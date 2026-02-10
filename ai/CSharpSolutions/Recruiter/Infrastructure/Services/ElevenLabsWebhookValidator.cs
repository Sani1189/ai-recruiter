using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recruiter.Application.ElevenLabs;
using Recruiter.Application.ElevenLabs.Interfaces;

public class ElevenLabsWebhookValidator : IElevenLabsWebhookValidator
{
    private readonly string _secret;
    private readonly ILogger<ElevenLabsWebhookValidator> _logger;
    private const int ToleranceMinutes = 30;

    public ElevenLabsWebhookValidator(
        IOptions<ElevenLabsOptions> options,
        ILogger<ElevenLabsWebhookValidator> logger)
    {
        _secret = options.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(_secret))
            throw new InvalidOperationException("WebhookSecret must be configured");
        _logger = logger;
    }

    public ElevenLabsValidationResult ValidateSignature(string? signatureHeader, string payload)
    {
        if (string.IsNullOrEmpty(signatureHeader))
            return new(false, "Missing signature header");

        // Parse "t=<timestamp>,v0=<signature>"
        var parts = signatureHeader.Split(',');
        if (parts.Length < 2)
            return new(false, "Invalid signature format");

        var timestamp = parts[0].StartsWith("t=") ? parts[0][2..] : null;
        var hmacSignature = parts[1];

        if (string.IsNullOrEmpty(timestamp) || !long.TryParse(timestamp, out var timestampValue))
            return new(false, "Invalid timestamp");

        // Validate timestamp (30-minute tolerance)
        var tolerance = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (ToleranceMinutes * 60);
        if (timestampValue < tolerance)
            return new(false, "Timestamp expired");

        // Validate HMAC-SHA256
        var fullPayloadToSign = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(fullPayloadToSign));
        var computedSignature = "v0=" + Convert.ToHexString(computedHash).ToLowerInvariant();

        if (hmacSignature != computedSignature)
        {
            _logger.LogWarning("Signature mismatch. Expected: {Expected}, Got: {Got}",
                computedSignature, hmacSignature);
            return new(false, "Invalid signature");
        }

        return new(true);
    }
}