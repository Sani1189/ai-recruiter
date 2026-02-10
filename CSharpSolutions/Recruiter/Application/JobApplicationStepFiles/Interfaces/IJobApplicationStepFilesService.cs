using Recruiter.Application.JobApplicationStepFiles.Dto;
using Ardalis.Result;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobApplicationStepFiles.Interfaces;

public interface IJobApplicationStepFilesService
{
    Task<IEnumerable<JobApplicationStepFilesDto>> GetAllAsync();
    Task<JobApplicationStepFilesDto?> GetByIdAsync(Guid id);
    Task<JobApplicationStepFilesDto> CreateAsync(JobApplicationStepFilesDto dto);
    Task<JobApplicationStepFilesDto> UpdateAsync(JobApplicationStepFilesDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<JobApplicationStepFilesDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<JobApplicationStepFilesDto>>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<Result<List<JobApplicationStepFilesDto>>> GetByJobApplicationStepIdAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsFileAttachedToStepAsync(Guid fileId, Guid jobApplicationStepId, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<JobApplicationStepFilesDto>>> GetFilteredJobApplicationStepFilesAsync(JobApplicationStepFilesListQueryDto query, CancellationToken cancellationToken = default);

    // Generic step file upload method - works for all step types (resume upload handled by strategy pattern)
    // If stepVersion is null, uses the latest version of the step
    Task<Result<UploadResumeResultDto>> UploadStepFileAsync(
        byte[] fileData, 
        string fileName, 
        string contentType, 
        string jobPostName, 
        int jobPostVersion,
        string stepName,
        int? stepVersion,
        Guid? candidateId = null, 
        CancellationToken cancellationToken = default);
    
    // Get upload SAS URL for direct client-to-Azure upload (best practice for user uploads)
    Task<Result<GetUploadUrlResponseDto>> GetUploadUrlAsync(GetUploadUrlRequestDto request, Guid? candidateId = null, CancellationToken cancellationToken = default);
    
    // Complete upload after direct upload to Azure (saves metadata to database)
    Task<Result<UploadResumeResultDto>> CompleteUploadAsync(CompleteUploadRequestDto request, Guid? candidateId = null, CancellationToken cancellationToken = default);
}
