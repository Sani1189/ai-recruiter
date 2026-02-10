using Recruiter.Application.JobApplication.Dto;
using Ardalis.Result;

namespace Recruiter.Application.JobApplication.Interfaces;

public interface IJobApplicationService
{
    // Admin methods
    Task<Result<Common.Dto.PagedResult<JobApplicationDto>>> GetFilteredJobApplicationsAsync(JobApplicationListQueryDto query, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<JobApplicationDto>>> GetByCandidateIdAsync(Guid candidateId, CancellationToken cancellationToken = default);
    Task<Result<List<JobApplicationDto>>> GetByJobPostAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default);
    Task<Result<List<JobApplicationDto>>> GetCompletedJobApplicationsAsync(CancellationToken cancellationToken = default);
    Task<Result<JobApplicationDto>> UpdateApplicationStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    
    // Candidate methods
    Task<Result<List<JobApplicationDto>>> GetMyApplicationsAsync(CancellationToken cancellationToken = default);
    Task<Result<Recruiter.Application.Common.Dto.PagedResult<JobApplicationDto>>> GetMyApplicationsPagedAsync(JobApplicationListQueryDto query, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationDto>> ApplyForJobAsync(ApplyForJobDto applyForJobDto, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationDto>> GetMyApplicationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<JobApplicationDto?>> GetMyApplicationByJobPostAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default);

    // Helper method for resume upload
    Task<Result<JobApplicationDto>> CreateOrGetJobApplicationAsync(string jobPostName, int jobPostVersion, Guid? candidateId = null, CancellationToken cancellationToken = default);
    // Get job application with steps and interviews for audio player
    Task<Result<JobApplicationWithStepsAndInterviewsDto>> GetJobApplicationWithStepsAndInterviewsAsync(string jobPostName, int jobPostVersion, Guid candidateId, CancellationToken cancellationToken = default);

    // Promote candidate to next step (admin/recruiter only)
    Task<Result<JobApplicationDto>> PromoteToNextStepAsync(Guid applicationId, PromoteStepDto promoteDto, CancellationToken cancellationToken = default);
}
