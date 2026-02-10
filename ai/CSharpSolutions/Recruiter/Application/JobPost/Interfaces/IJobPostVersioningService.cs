using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Interfaces;

/// <summary>
/// Service for handling versioning operations for JobPost and JobPostStep entities
/// </summary>
public interface IJobPostVersioningService
{
    /// <summary>
    /// Creates a new version of a JobPost and copies all step assignments to the new version
    /// </summary>
    /// <param name="dto">JobPost data for the new version</param>
    /// <param name="nextVersion">Version number for the new JobPost</param>
    /// <returns>New JobPost version</returns>
    Task<JobPostDto> CreateJobPostVersionAsync(JobPostDto dto, int nextVersion);

    /// <summary>
    /// Creates a new version of a JobPostStep and cascades updates to all related JobPosts
    /// </summary>
    /// <param name="dto">JobPostStep data for the new version</param>
    /// <param name="nextVersion">Version number for the new JobPostStep</param>
    /// <returns>New JobPostStep version</returns>
    Task<JobPostStepDto> CreateJobPostStepVersionAsync(JobPostStepDto dto, int nextVersion);

    /// <summary>
    /// Gets the next version number for a JobPost
    /// </summary>
    /// <param name="name">JobPost name</param>
    /// <returns>Next version number</returns>
    Task<int> GetNextJobPostVersionAsync(string name);

    /// <summary>
    /// Gets the next version number for a JobPostStep
    /// </summary>
    /// <param name="name">JobPostStep name</param>
    /// <returns>Next version number</returns>
    Task<int> GetNextJobPostStepVersionAsync(string name);
}
