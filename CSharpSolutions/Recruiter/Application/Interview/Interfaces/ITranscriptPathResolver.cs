namespace Recruiter.Application.Interview.Interfaces;

/// <summary>
/// Resolves the blob storage path for interview transcripts based on conversation_id
/// </summary>
public interface ITranscriptPathResolver
{
    /// <summary>
    /// Resolves the storage path for a transcript based on ElevenLabs conversation_id.
    /// Queries: Interview (by TranscriptUrl) → JobApplicationStep → JobApplication
    /// </summary>
    /// <param name="conversationId">The ElevenLabs conversation_id from the webhook</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path result with job application details, or null if not found</returns>
    Task<TranscriptPathResult?> ResolvePathAsync(string conversationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result containing the resolved blob path and job application metadata
/// </summary>
public record TranscriptPathResult(
    string BlobPath,
    Guid JobApplicationId,
    string JobPostName,
    int JobPostVersion);
