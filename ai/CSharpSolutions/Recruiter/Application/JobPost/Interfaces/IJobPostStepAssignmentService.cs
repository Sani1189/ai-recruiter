using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Interfaces;

public interface IJobPostStepAssignmentService
{
    Task<IEnumerable<JobPostStepAssignmentDto>> GetByJobPostAsync(string jobPostName, int jobPostVersion);
    Task<JobPostStepAssignmentDto> AssignStepAsync(JobPostStepAssignmentDto dto);
    Task UnassignStepAsync(string jobPostName, int jobPostVersion, string stepName, int? stepVersion);
    Task UpdateAssignmentStatusAsync(string jobPostName, int jobPostVersion, string stepName, int? stepVersion, string status);
}