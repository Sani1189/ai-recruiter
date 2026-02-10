using Recruiter.Application.JobApplication.Dto;
using Ardalis.Result;

namespace Recruiter.Application.JobApplication.Interfaces;

public interface IJobApplicationStepService
{
    // Admin methods
    Task<Result<JobApplicationStepDto>> CreateAsync(JobApplicationStepDto stepDto, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationStepDto>> UpdateAsync(JobApplicationStepDto stepDto, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<JobApplicationStepDto>>> GetFilteredJobApplicationStepsAsync(JobApplicationStepListQueryDto query, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationStepDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<JobApplicationStepDto>>> GetByJobApplicationIdAsync(Guid jobApplicationId, CancellationToken cancellationToken = default);
    Task<Result<List<JobApplicationStepDto>>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationStepDto>> UpdateStepStatusAsync(Guid stepId, string status, CancellationToken cancellationToken = default);
    
    // Candidate methods
    Task<Result<List<JobApplicationStepDto>>> GetMyApplicationStepsAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationStepDto>> UpdateMyStepAsync(Guid stepId, JobApplicationStepDto stepDto, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationStepDto>> CompleteMyStepAsync(Guid stepId, string? data, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationStepDto>> StartMyStepAsync(Guid stepId, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationStepDto>> CreateMyStepAsync(Guid applicationId, CreateStepDto createDto, CancellationToken cancellationToken = default);
}