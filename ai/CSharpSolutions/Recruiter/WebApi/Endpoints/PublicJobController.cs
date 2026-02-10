using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Exceptions;
using Recruiter.Application.JobPost.Interfaces;

namespace Recruiter.WebApi.Endpoints;

// Public endpoints for job posts (no authentication required).
// Only Published jobs are returned; status is enforced in query specs. Recruiter/admin use authenticated endpoints.
[ApiController]
[Route("api/public/job")]
[AllowAnonymous]
public class PublicJobController(IJobPostOrchestrator jobPostOrchestrator) : ControllerBase
{
    private readonly IJobPostOrchestrator _jobPostOrchestrator = jobPostOrchestrator;

    [HttpGet("{name}/{version}")]
    public async Task<ActionResult<JobPostDto>> GetJobPost(string name, int version)
    {
        try
        {
            var jobPost = await _jobPostOrchestrator.GetPublishedJobPostWithStepsAsync(name, version);
            if (jobPost == null)
                return NotFound(new { message = "Job post not found", errorCode = "NOT_FOUND" });
            return Ok(jobPost);
        }
        catch (JobPostNotAvailableException ex)
        {
            return NotFound(new { message = ex.Message, errorCode = JobPostNotAvailableException.ErrorCode });
        }
    }

    [HttpGet("{name}/latest")]
    public async Task<ActionResult<JobPostDto>> GetLatestJobPost(string name)
    {
        try
        {
            var jobPost = await _jobPostOrchestrator.GetLatestPublishedJobPostWithStepsAsync(name);
            if (jobPost == null)
                return NotFound(new { message = "Job post not found", errorCode = "NOT_FOUND" });
            return Ok(jobPost);
        }
        catch (JobPostNotAvailableException ex)
        {
            return NotFound(new { message = ex.Message, errorCode = JobPostNotAvailableException.ErrorCode });
        }
    }
}


