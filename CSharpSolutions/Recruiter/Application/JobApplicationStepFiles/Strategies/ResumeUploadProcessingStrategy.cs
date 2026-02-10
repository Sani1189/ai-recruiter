using Microsoft.Extensions.Logging;
using Recruiter.Application.CvProcessing.Interfaces;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Options;
using Recruiter.Application.Prompt.Interfaces;

namespace Recruiter.Application.JobApplicationStepFiles.Strategies;

// Strategy for processing resume uploads - handles CV extraction via Python API
public class ResumeUploadProcessingStrategy : IFileUploadProcessingStrategy
{
    private readonly ICvProcessingService _cvProcessingService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPromptService _promptService;
    private readonly AzureStorageOptions _storageOptions;
    private readonly ILogger<ResumeUploadProcessingStrategy> _logger;

    private const string ResumeUploadStepType = "resume upload";
    private const string DefaultPromptCategory = "cv_extraction";
    private const string DefaultPromptName = "CVExtractionScoringInstructions";
    private const int DefaultPromptVersion = 1;

    public ResumeUploadProcessingStrategy(
        ICvProcessingService cvProcessingService,
        IFileStorageService fileStorageService,
        IPromptService promptService,
        AzureStorageOptions storageOptions,
        ILogger<ResumeUploadProcessingStrategy> logger)
    {
        _cvProcessingService = cvProcessingService ?? throw new ArgumentNullException(nameof(cvProcessingService));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _promptService = promptService ?? throw new ArgumentNullException(nameof(promptService));
        _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanHandle(string stepType) => string.Equals(stepType, ResumeUploadStepType, StringComparison.OrdinalIgnoreCase);

    public async Task ProcessFileUploadAsync(
        FileUploadProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.CandidateUserId == Guid.Empty)
        {
            _logger.LogWarning("Skipping CV extraction: Invalid candidate user ID for file {FileName}", context.FileName);
            return;
        }

        try
        {
            await using var stream = await GetFileStreamAsync(context, cancellationToken);
            
            // Use prompt from step if available, otherwise use default
            var promptCategory = context.PromptCategory ?? DefaultPromptCategory;
            var promptName = context.PromptName ?? DefaultPromptName;
            
            // Resolve prompt version: if null, get latest version; otherwise use provided version
            var promptVersion = await ResolvePromptVersionAsync(promptName, context.PromptVersion, cancellationToken);

            // Log prompt parameters before calling Python API
            _logger.LogInformation(
                "Calling Python API for CV extraction - File: {FileName}, CandidateId: {CandidateId}, PromptCategory: {PromptCategory}, PromptName: {PromptName}, PromptVersion: {PromptVersion}",
                context.FileName,
                context.CandidateUserId,
                promptCategory,
                promptName,
                promptVersion);

            var result = await _cvProcessingService.UploadCvAsync(
                stream,
                context.FileName,
                context.ContentType,
                context.CandidateUserId,
                promptCategory,
                promptName,
                promptVersion,
                cancellationToken);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Errors?.FirstOrDefault()
                    ?? result.ValidationErrors?.FirstOrDefault()?.ErrorMessage
                    ?? "Unknown error";
                _logger.LogError("CV extraction failed for {FileName}: {ErrorMessage}", context.FileName, errorMessage);
            }
            else
            {
                _logger.LogInformation("CV extraction request sent successfully for {FileName}", context.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering CV extraction for {FileName}: {ErrorMessage}", context.FileName, ex.Message);
            // Don't throw - file upload succeeded, processing failure is logged
        }
    }

    private async Task<Stream> GetFileStreamAsync(
        FileUploadProcessingContext context,
        CancellationToken cancellationToken)
    {
        // Use file data if available (direct upload), otherwise download from storage
        if (context.FileData is { Length: > 0 })
        {
            return new MemoryStream(context.FileData);
        }

        var downloadedStream = await _fileStorageService.DownloadAsync(
            _storageOptions.ContainerName,
            context.BlobPath);

        var memoryStream = new MemoryStream();
        await downloadedStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task<int> ResolvePromptVersionAsync(
        string promptName,
        int? promptVersion,
        CancellationToken cancellationToken)
    {
        // If version is provided, use it
        if (promptVersion.HasValue)
            return promptVersion.Value;

        // If no version provided, get latest version
        if (!string.IsNullOrWhiteSpace(promptName))
        {
            var latestPromptResult = await _promptService.GetLatestByNameAsync(promptName, cancellationToken);
            if (latestPromptResult.IsSuccess && latestPromptResult.Value != null)
            {
                return latestPromptResult.Value.Version;
            }
        }

        // Fallback to default version if prompt not found
        return DefaultPromptVersion;
    }
}

