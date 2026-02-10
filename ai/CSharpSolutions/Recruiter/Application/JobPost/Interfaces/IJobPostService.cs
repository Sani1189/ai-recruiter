using Recruiter.Application.Common.Dto;
using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Interfaces;

public interface IJobPostService
{
    Task<JobPostDto?> GetByIdAsync(string name, int version);
    Task<JobPostDto?> GetLatestVersionAsync(string name);
    /// <summary>Published only; throws when job exists but is not published.</summary>
    Task<JobPostDto?> GetPublishedByIdAsync(string name, int version);
    /// <summary>Latest published version only; throws when job exists but has no published version.</summary>
    Task<JobPostDto?> GetLatestPublishedVersionAsync(string name);
    Task<IEnumerable<JobPostDto>> GetEditHistoryAsync(string name);
    Task<PagedResult<JobPostDto>> GetListAsync(JobPostListQueryDto query);
    Task<JobPostDto> CreateAsync(JobPostDto dto);
    Task<JobPostDto> UpdateAsync(JobPostDto dto);
    Task<bool> ExistsAsync(string name, int version);
    
    // New methods for candidate-related functionality
    Task<PagedResult<JobPostWithCandidatesDto>> GetListWithCandidateCountsAsync(JobPostListQueryDto query);
    Task<JobPostWithCandidatesDto?> GetByIdWithCandidatesAsync(string name, int version);
}
