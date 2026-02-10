using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobPost.Dto;

/// <summary>
/// DTO for job post with candidate count
/// </summary>
public class JobPostWithCandidatesDto : JobPostDto
{
    /// <summary>
    /// Number of candidates who applied for this job post
    /// </summary>
    public int CandidateCount { get; set; } = 0;
}
