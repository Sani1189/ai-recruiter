namespace Recruiter.Application.ElevenLabs.Interfaces;

public interface IElevenLabsWebhookValidator
{
    ElevenLabsValidationResult ValidateSignature(string? signatureHeader, string payload);
}

public record ElevenLabsValidationResult(bool IsValid, string? ErrorMessage = null);