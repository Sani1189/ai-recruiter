using Recruiter.Application.JobApplicationStepFiles.Dto;

namespace Recruiter.Application.JobApplicationStepFiles.Strategies;

// Strategy pattern for processing file uploads based on step type.
// Different strategies handle different processing requirements (e.g., CV extraction, prompt-based processing).
public interface IFileUploadProcessingStrategy
{
    // Determines if this strategy can handle the given step type
    bool CanHandle(string stepType);

    // Processes the file upload after it has been stored (may call external APIs like Python API for CV processing)
    Task ProcessFileUploadAsync(
        FileUploadProcessingContext context,
        CancellationToken cancellationToken = default);
}

public record FileUploadProcessingContext(
    Guid JobApplicationId,
    Guid JobApplicationStepId,
    Guid FileId,
    Guid CandidateUserId,
    string BlobPath,
    string FileName,
    string ContentType,
    byte[]? FileData,
    string? PromptCategory,
    string? PromptName,
    int? PromptVersion);

