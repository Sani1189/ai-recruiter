using Ardalis.Result;
using Recruiter.Application.JobApplication.Dto;

namespace Recruiter.Application.JobApplication.Interfaces;

public interface IJobApplicationCandidateFlowService
{
    Task<Result<JobApplicationDto?>> GetMyApplicationByJobPostAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default);

    Task<Result<MyJobApplicationProgressDto>> GetMyProgressAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default);

    Task<Result<BeginJobApplicationStepResponseDto>> BeginStepAsync(string jobPostName, int jobPostVersion, BeginJobApplicationStepRequestDto request, CancellationToken cancellationToken = default);
}



