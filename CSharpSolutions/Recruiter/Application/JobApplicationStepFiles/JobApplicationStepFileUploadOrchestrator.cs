using Microsoft.Extensions.DependencyInjection;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Options;
using Recruiter.Application.Common.Helpers;
using Recruiter.Application.File.Interfaces;
using Recruiter.Application.JobApplicationStepFiles.Services;
using Recruiter.Application.JobApplicationStepFiles.Strategies;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles;

// Orchestrates file uploads for job application steps.
// Handles file storage, step management, and delegates processing to appropriate strategies.
public class JobApplicationStepFileUploadOrchestrator
{
    private readonly IFileService _fileService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IJobApplicationStepResolver _stepResolver;
    private readonly IRepository<Domain.Models.JobApplicationStepFiles> _stepFilesRepository;
    private readonly AzureStorageOptions _storageOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public JobApplicationStepFileUploadOrchestrator(
        IFileService fileService,
        IFileStorageService fileStorageService,
        IJobApplicationStepResolver stepResolver,
        IRepository<Domain.Models.JobApplicationStepFiles> stepFilesRepository,
        AzureStorageOptions storageOptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _stepResolver = stepResolver ?? throw new ArgumentNullException(nameof(stepResolver));
        _stepFilesRepository = stepFilesRepository ?? throw new ArgumentNullException(nameof(stepFilesRepository));
        _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task<FileUploadResult> ProcessFileUploadAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Create file record
        var (folderName, fileName) =
        ExtractPathParts(request.BlobPath, request.FileInfo.SafeFileName);
        var createdFile = await CreateFileRecordAsync(folderName, fileName, request.FileInfo, cancellationToken);
        var fileId = createdFile.Id ?? throw new InvalidOperationException("File record was not created.");

        // 2. Get or create job application step
        var jobApplicationStep = await _stepResolver.GetOrCreateStepAsync(
            request.JobApplicationId,
            request.StepName,
            request.StepVersion,
            cancellationToken);

        // 3. Link file to step
        var stepFilesId = await UpsertJobApplicationStepFileAsync(
            jobApplicationStep.Id,
            fileId,
            cancellationToken);

        // 4. Get JobPostStep to determine step type and prompt info
        var jobPostStep = await _stepResolver.GetJobPostStepAsync(
            request.StepName,
            request.StepVersion,
            cancellationToken);

        var stepType = jobPostStep?.StepType ?? "Resume Upload"; // Default for backward compatibility
        var promptName = jobPostStep?.PromptName;
        var promptCategory = jobPostStep?.Prompt?.Category;
        var promptVersion = jobPostStep?.PromptVersion;

        // 5. Process file upload using appropriate strategy (fire and forget for async processing)
        var processingContext = new Strategies.FileUploadProcessingContext(
            request.JobApplicationId,
            jobApplicationStep.Id,
            fileId,
            request.UserId,
            request.BlobPath,
            request.FileInfo.SafeFileName,
            request.FileInfo.ContentType,
            request.FileData,
            promptCategory,
            promptName,
            promptVersion);

        _ = ProcessFileUploadWithStrategyAsync(stepType, processingContext, cancellationToken);

        return new FileUploadResult(
            jobApplicationStep.Id,
            fileId,
            stepFilesId,
            request.FileInfo.SafeFileName,
            request.BlobPath,
            request.FileInfo.FileSizeBytes);
    }

    private async Task ProcessFileUploadWithStrategyAsync(
        string stepType,
        Strategies.FileUploadProcessingContext context,
        CancellationToken cancellationToken)
    {
        // Create a new service scope for background processing to avoid DbContext disposal issues
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var processingStrategies = scope.ServiceProvider.GetRequiredService<IEnumerable<IFileUploadProcessingStrategy>>();
        
        try
        {
            var strategy = processingStrategies.FirstOrDefault(s => s.CanHandle(stepType))
                ?? processingStrategies.FirstOrDefault(s => s is GenericFileUploadProcessingStrategy);

            if (strategy != null)
            {
                await strategy.ProcessFileUploadAsync(context, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file upload with strategy: {ex.Message}");
            // Don't throw - file upload succeeded, processing failure is logged
        }
    }

    private async Task<File.Dto.FileDto> CreateFileRecordAsync(
        string folderName,
        string fileName,
        FileInfo fileInfo,
        CancellationToken cancellationToken)
    {
        var storageAccountName = _fileStorageService.GetStorageAccountName();
        if (string.IsNullOrWhiteSpace(storageAccountName))
            throw new InvalidOperationException("Storage account name could not be determined.");

        var fileDto = new Recruiter.Application.File.Dto.FileDto
        {
            Container = _storageOptions.ContainerName,
            FolderPath = folderName,
            FilePath = fileName,
            Extension = fileInfo.Extension,
            MbSize = fileInfo.FileSizeMb,
            StorageAccountName = storageAccountName
        };

        var createdFile = await _fileService.CreateAsync(fileDto);
        if (!createdFile.Id.HasValue)
            throw new InvalidOperationException("File record was not created.");

        return createdFile;
    }

    private async Task<Guid> UpsertJobApplicationStepFileAsync(
        Guid jobApplicationStepId,
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var existingStepFiles = (await _stepFilesRepository.ListAsync(
                new Specifications.JobApplicationStepFilesByJobApplicationStepIdSpec(jobApplicationStepId),
                cancellationToken))
            .ToList();

        if (existingStepFiles.Any())
        {
            // Update existing step files to point to new file
            foreach (var stepFile in existingStepFiles)
            {
                stepFile.FileId = fileId;
                await _stepFilesRepository.UpdateAsync(stepFile);
            }
            await _stepFilesRepository.SaveChangesAsync(cancellationToken);
            return existingStepFiles.First().Id;
        }

        // Create new step file link
        var jobApplicationStepFile = new Domain.Models.JobApplicationStepFiles
        {
            JobApplicationStepId = jobApplicationStepId,
            FileId = fileId
        };
        await _stepFilesRepository.AddAsync(jobApplicationStepFile);
        await _stepFilesRepository.SaveChangesAsync(cancellationToken);
        return jobApplicationStepFile.Id;
    }

    private static (string FolderName, string FileName) ExtractPathParts(string blobPath, string fallbackFileName)
        => FileHelper.ExtractPathParts(blobPath, fallbackFileName);
}

public record FileInfo(string SafeFileName, string Extension, string ContentType, long FileSizeBytes, int FileSizeMb);

public record FileUploadRequest(
    Guid JobApplicationId,
    Guid UserId,
    string BlobPath,
    FileInfo FileInfo,
    string StepName,
    int? StepVersion, // Null means use latest version
    byte[]? FileData = null);

public record FileUploadResult(
    Guid JobApplicationStepId,
    Guid FileId,
    Guid JobApplicationStepFilesId,
    string FileName,
    string FileUrl,
    long FileSize);
