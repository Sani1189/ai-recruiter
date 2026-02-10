using Recruiter.Application.Common.Dto;
using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Interfaces;

public interface IJobPostOrchestrator
{
    Task<JobPostDto> CreateJobPostWithStepsAsync(JobPostDto jobPostDto);
    Task<JobPostDto?> GetJobPostWithStepsAsync(string name, int version);
    /// <summary>Published only for public/candidate; throws when job exists but is not published.</summary>
    Task<JobPostDto?> GetPublishedJobPostWithStepsAsync(string name, int version);
    /// <summary>Latest published version with steps for public/candidate.</summary>
    Task<JobPostDto?> GetLatestPublishedJobPostWithStepsAsync(string name);
    Task<IEnumerable<JobPostDto>> GetJobPostsWithStepsAsync(IEnumerable<JobPostDto> jobPosts);
    Task<PagedResult<JobPostDto>> GetJobPostsWithStepsPagedAsync(JobPostListQueryDto query);
    Task<JobPostDto> UpdateJobPostWithStepsAsync(JobPostDto jobPostDto);
    Task<JobPostDto?> DuplicateJobPostWithStepsAsync(string sourceName, int sourceVersion, DuplicateJobPostRequestDto request);
    Task<Ardalis.Result.Result> DeleteJobPostWithStepsAsync(string name, int version);
}