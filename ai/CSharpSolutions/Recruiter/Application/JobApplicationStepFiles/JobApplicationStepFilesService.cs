using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Options;
using Recruiter.Application.Common.Helpers;
using Recruiter.Application.JobApplicationStepFiles.Dto;
using Recruiter.Application.JobApplicationStepFiles.Interfaces;
using Recruiter.Application.JobApplicationStepFiles.Queries;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.Candidate.Interfaces;
using Ardalis.Result;

namespace Recruiter.Application.JobApplicationStepFiles;

// Service for managing job application step files.
// Handles file uploads for job application steps with support for different step types and processing strategies.
public class JobApplicationStepFilesService : IJobApplicationStepFilesService
{
    private readonly IRepository<Domain.Models.JobApplicationStepFiles> _repository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IJobApplicationService _jobApplicationService;
    private readonly ICandidateService _candidateService;
    private readonly JobApplicationStepFileUploadOrchestrator _uploadOrchestrator;
    private readonly AzureStorageOptions _storageOptions;
    private readonly IMapper _mapper;
    private readonly JobApplicationStepFilesQueryHandler _queryHandler;

    private const int SasUrlExpirationMinutes = 20;

    public JobApplicationStepFilesService(
        IRepository<Domain.Models.JobApplicationStepFiles> repository,
        IFileStorageService fileStorageService,
        IJobApplicationService jobApplicationService,
        ICandidateService candidateService,
        JobApplicationStepFileUploadOrchestrator uploadOrchestrator,
        AzureStorageOptions storageOptions,
        IMapper mapper,
        JobApplicationStepFilesQueryHandler queryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _jobApplicationService = jobApplicationService ?? throw new ArgumentNullException(nameof(jobApplicationService));
        _candidateService = candidateService ?? throw new ArgumentNullException(nameof(candidateService));
        _uploadOrchestrator = uploadOrchestrator ?? throw new ArgumentNullException(nameof(uploadOrchestrator));
        _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    public async Task<IEnumerable<JobApplicationStepFilesDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<JobApplicationStepFilesDto>>(entities);
    }

    public async Task<JobApplicationStepFilesDto?> GetByIdAsync(Guid id)
    {
        var result = await _queryHandler.GetByIdAsync(id);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<JobApplicationStepFilesDto> CreateAsync(JobApplicationStepFilesDto dto)
    {
        var entity = _mapper.Map<Domain.Models.JobApplicationStepFiles>(dto);
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        return _mapper.Map<JobApplicationStepFilesDto>(entity);
    }

    public async Task<JobApplicationStepFilesDto> UpdateAsync(JobApplicationStepFilesDto dto)
    {
        var entity = _mapper.Map<Domain.Models.JobApplicationStepFiles>(dto);
        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();
        return _mapper.Map<JobApplicationStepFilesDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity != null)
        {
            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null;
    }

    public async Task<Result<JobApplicationStepFilesDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _queryHandler.GetByIdAsync(id, cancellationToken);

    public async Task<Result<List<JobApplicationStepFilesDto>>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
        => await _queryHandler.GetByFileIdAsync(fileId, cancellationToken);

    public async Task<Result<List<JobApplicationStepFilesDto>>> GetByJobApplicationStepIdAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default)
        => await _queryHandler.GetByJobApplicationStepIdAsync(jobApplicationStepId, cancellationToken);

    public async Task<Result<bool>> IsFileAttachedToStepAsync(Guid fileId, Guid jobApplicationStepId, CancellationToken cancellationToken = default)
        => await _queryHandler.IsFileAttachedToStepAsync(fileId, jobApplicationStepId, cancellationToken);

    public async Task<Result<Common.Dto.PagedResult<JobApplicationStepFilesDto>>> GetFilteredJobApplicationStepFilesAsync(JobApplicationStepFilesListQueryDto query, CancellationToken cancellationToken = default)
        => await _queryHandler.GetFilteredJobApplicationStepFilesAsync(query, cancellationToken);

    public async Task<Result<UploadResumeResultDto>> UploadStepFileAsync(
        byte[] fileData, 
        string originalFileName, 
        string contentType, 
        string jobPostName, 
        int jobPostVersion,
        string stepName,
        int? stepVersion,
        Guid? candidateId = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(stepName))
                return Result<UploadResumeResultDto>.Invalid(
                    new ValidationError { ErrorMessage = "Step name is required" });

            if (stepVersion.HasValue && stepVersion < 1)
                return Result<UploadResumeResultDto>.Invalid(
                    new ValidationError { ErrorMessage = "Step version must be greater than 0" });

            var context = await GetUploadContextAsync(jobPostName, jobPostVersion, candidateId, cancellationToken);
            var fileInfo = ProcessFileInfo(originalFileName, contentType, fileData.LongLength);
            
            var (folderName, fileName) = BlobFilePathHelper.BuildJobApplicationPath(
                jobPostName, 
                jobPostVersion, 
                context.JobApplicationId, 
                fileInfo.Extension);
            var fullBlobPath = BlobFilePathHelper.GetFullBlobPath(folderName, fileName);
            
            using var stream = new MemoryStream(fileData);
            await _fileStorageService.UploadAsync(_storageOptions.ContainerName, fullBlobPath, stream, fileInfo.ContentType);

            var uploadRequest = new FileUploadRequest(
                context.JobApplicationId,
                context.CandidateUserId,
                fullBlobPath,
                fileInfo,
                stepName,
                stepVersion,
                fileData);

            var result = await _uploadOrchestrator.ProcessFileUploadAsync(uploadRequest, cancellationToken);

            return Result<UploadResumeResultDto>.Success(new UploadResumeResultDto
            {
                JobApplicationStepId = result.JobApplicationStepId,
                FileId = result.FileId,
                JobApplicationStepFilesId = result.JobApplicationStepFilesId,
                FileName = result.FileName,
                FileUrl = result.FileUrl,
                FileSize = result.FileSize
            });
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload file: {ex.Message}", ex);
        }
    }


    public async Task<Result<GetUploadUrlResponseDto>> GetUploadUrlAsync(
        GetUploadUrlRequestDto request, 
        Guid? candidateId = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request?.StepName))
            return Result<GetUploadUrlResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Step name is required" });

        if (request.StepVersion.HasValue && request.StepVersion < 1)
            return Result<GetUploadUrlResponseDto>.Invalid(
                new ValidationError { ErrorMessage = "Step version must be greater than 0" });

        try
        {
            var (_, extensionWithDot) = FileHelper.SanitizeFileName(request.FileName);
            var extension = extensionWithDot.TrimStart('.');
            var context = await GetUploadContextAsync(
                request.JobPostName, 
                request.JobPostVersion, 
                candidateId, 
                cancellationToken);
            
            var (folderName, fileName) = BlobFilePathHelper.BuildJobApplicationPath(
                request.JobPostName, 
                request.JobPostVersion, 
                context.JobApplicationId, 
                extension);
            var fullBlobPath = BlobFilePathHelper.GetFullBlobPath(folderName, fileName);
            
            var uploadUrl = _fileStorageService.GenerateUploadSasUrl(
                _storageOptions.ContainerName, 
                fullBlobPath, 
                SasUrlExpirationMinutes);

            return Result<GetUploadUrlResponseDto>.Success(new GetUploadUrlResponseDto
            {
                UploadUrl = uploadUrl,
                BlobPath = fullBlobPath,
                ExpiresInMinutes = SasUrlExpirationMinutes
            });
        }
        catch (InvalidOperationException ex)
        {
            return Result<GetUploadUrlResponseDto>.Invalid(
                new ValidationError { ErrorMessage = ex.Message });
        }
        catch (Exception ex)
        {
            return Result<GetUploadUrlResponseDto>.Invalid(
                new ValidationError { ErrorMessage = $"Failed to generate upload URL: {ex.Message}" });
        }
    }

    public async Task<Result<UploadResumeResultDto>> CompleteUploadAsync(
        CompleteUploadRequestDto request, 
        Guid? candidateId = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request?.StepName))
            return Result<UploadResumeResultDto>.Invalid(
                new ValidationError { ErrorMessage = "Step name is required" });

        if (request.StepVersion.HasValue && request.StepVersion < 1)
            return Result<UploadResumeResultDto>.Invalid(
                new ValidationError { ErrorMessage = "Step version must be greater than 0" });

        try
        {
            var context = await GetUploadContextAsync(
                request.JobPostName, 
                request.JobPostVersion, 
                candidateId, 
                cancellationToken);
            
            var fileExists = await _fileStorageService.ExistsAsync(
                _storageOptions.ContainerName, 
                request.BlobPath);
            
            if (!fileExists)
            {
                return Result<UploadResumeResultDto>.Invalid(
                    new ValidationError { ErrorMessage = "File was not found in storage. Please ensure the upload completed successfully." });
            }

            var fileInfo = ProcessFileInfo(request.OriginalFileName, request.ContentType, request.FileSize);
            
            var uploadRequest = new FileUploadRequest(
                context.JobApplicationId,
                context.CandidateUserId,
                request.BlobPath,
                fileInfo,
                request.StepName,
                request.StepVersion);

            var result = await _uploadOrchestrator.ProcessFileUploadAsync(uploadRequest, cancellationToken);

            return Result<UploadResumeResultDto>.Success(new UploadResumeResultDto
            {
                JobApplicationId = context.JobApplicationId,
                JobApplicationStepId = result.JobApplicationStepId,
                FileId = result.FileId,
                JobApplicationStepFilesId = result.JobApplicationStepFilesId,
                FileName = result.FileName,
                FileUrl = result.FileUrl,
                FileSize = result.FileSize
            });
        }
        catch (InvalidOperationException ex)
        {
            return Result<UploadResumeResultDto>.Invalid(
                new ValidationError { ErrorMessage = ex.Message });
        }
        catch (Exception ex)
        {
            return Result<UploadResumeResultDto>.Invalid(
                new ValidationError { ErrorMessage = $"Failed to complete upload: {ex.Message}" });
        }
    }

    private FileInfo ProcessFileInfo(string originalFileName, string contentType, long fileSizeBytes)
    {
        var (safeFileName, extensionWithDot) = FileHelper.SanitizeFileName(originalFileName);
        var extension = extensionWithDot.TrimStart('.');
        var contentTypeResolved = FileHelper.ResolveContentType(contentType, extensionWithDot);
        var fileSizeMb = FileHelper.CalculateFileSizeInMb(fileSizeBytes);
        
        return new FileInfo(safeFileName, extension, contentTypeResolved, fileSizeBytes, fileSizeMb);
    }

    private async Task<UploadContext> GetUploadContextAsync(
        string jobPostName, 
        int jobPostVersion, 
        Guid? candidateId, 
        CancellationToken cancellationToken)
    {
        var jobApplication = await _jobApplicationService.CreateOrGetJobApplicationAsync(
            jobPostName, 
            jobPostVersion, 
            candidateId, 
            cancellationToken);
        
        if (!jobApplication.IsSuccess || jobApplication.Value?.Id == null || !jobApplication.Value.CandidateId.HasValue)
            throw new InvalidOperationException("Unable to resolve job application for the current candidate.");

        var candidate = await _candidateService.GetByIdWithUserProfileAsync(jobApplication.Value.CandidateId.Value);
        if (candidate == null || !candidate.Id.HasValue)
            throw new InvalidOperationException("Candidate record not found for the current job application.");

        return new UploadContext(jobApplication.Value.Id.Value, candidate.UserId);
    }


    private record UploadContext(Guid JobApplicationId, Guid CandidateUserId);
}
