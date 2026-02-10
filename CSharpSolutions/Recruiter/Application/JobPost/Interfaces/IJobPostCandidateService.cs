using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Interfaces;

public interface IJobPostCandidateService
{
    /// Get candidate counts for multiple job posts
    Task<Dictionary<string, int>> GetCandidateCountsAsync(IEnumerable<(string Name, int Version)> jobPosts, CancellationToken cancellationToken = default);
    
    /// Get candidates for a specific job post
    Task<IEnumerable<JobPostCandidateDto>> GetJobPostCandidatesAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default);
    
    /// Get all candidates for admin view
    Task<IEnumerable<JobPostCandidateDto>> GetAllCandidatesAsync(CancellationToken cancellationToken = default);
}
