using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrCandidate")]
public class JobController(
    IJobPostService jobPostService,
    IJobPostOrchestrator jobPostOrchestrator,
    IJobPostCandidateService candidateService) : ControllerBase
{
    private readonly IJobPostService _jobPostService = jobPostService;
    private readonly IJobPostOrchestrator _jobPostOrchestrator = jobPostOrchestrator;
    private readonly IJobPostCandidateService _candidateService = candidateService;

    // Get job post by name and version
    [HttpGet("{name}/{version}")]
    public async Task<ActionResult<JobPostDto>> GetJobPost(string name, int version)
    {
        var jobPost = await _jobPostOrchestrator.GetJobPostWithStepsAsync(name, version);
        if (jobPost == null)
            return NotFound();

        return Ok(jobPost);
    }

    // Get latest version of job post by name
    [HttpGet("{name}/latest")]
    public async Task<ActionResult<JobPostDto>> GetLatestJobPost(string name)
    {
        // First get the latest version number
        var latestJobPost = await _jobPostService.GetLatestVersionAsync(name);
        if (latestJobPost == null)
            return NotFound();

        // Then get it with step assignments using orchestrator
        var jobPostWithSteps = await _jobPostOrchestrator.GetJobPostWithStepsAsync(name, latestJobPost.Version);
        if (jobPostWithSteps == null)
            return NotFound();

        return Ok(jobPostWithSteps);
    }

    // Get edit history by name
    [HttpGet("{name}/history")]
    public async Task<ActionResult<IEnumerable<JobPostDto>>> GetEditHistory(string name)
    {
        var history = await _jobPostService.GetEditHistoryAsync(name);
        return Ok(history);
    }

    // Get job posts (simple list - supports filtering but no pagination)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobPostDto>>> GetJobPosts([FromQuery] JobPostListQueryDto query, [FromQuery] bool includeSteps = false)
    {
        // Set pagination to get all results
        query.Page = 1;
        query.PageSize = int.MaxValue;

        if (includeSteps)
        {
            var jobPostsWithSteps = await _jobPostOrchestrator.GetJobPostsWithStepsPagedAsync(query);
            return Ok(jobPostsWithSteps.Items);
        }
        else
        {
            var jobPosts = await _jobPostService.GetListAsync(query);
            return Ok(jobPosts.Items);
        }
    }

    // Get filtered job posts with pagination and filters
    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<JobPostDto>>> GetFilteredJobPosts([FromQuery] JobPostListQueryDto query, [FromQuery] bool includeSteps = false)
    {
        if (includeSteps)
        {
            var jobPostsWithSteps = await _jobPostOrchestrator.GetJobPostsWithStepsPagedAsync(query);
            return Ok(jobPostsWithSteps);
        }
        else
        {
            var jobPosts = await _jobPostService.GetListAsync(query);
            return Ok(jobPosts);
        }
    }

    // Get filtered job posts with pagination, filters, and steps (complete data)
    [HttpGet("filtered/with-steps")]
    public async Task<ActionResult<PagedResult<JobPostDto>>> GetFilteredJobPostsWithSteps([FromQuery] JobPostListQueryDto query)
    {
        var jobPostsWithSteps = await _jobPostOrchestrator.GetJobPostsWithStepsPagedAsync(query);
        return Ok(jobPostsWithSteps);
    }

    // Search by seniority level (with steps)
    [HttpGet("search/experience/{experienceLevel}")]
    public async Task<ActionResult<PagedResult<JobPostDto>>> GetJobPostsByExperienceLevel(string experienceLevel, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new JobPostListQueryDto { ExperienceLevel = experienceLevel, Page = page, PageSize = pageSize };
        var jobPostsWithSteps = await _jobPostOrchestrator.GetJobPostsWithStepsPagedAsync(query);
        return Ok(jobPostsWithSteps);
    }

    // Search by job type (with steps)
    [HttpGet("search/type/{jobType}")]
    public async Task<ActionResult<PagedResult<JobPostDto>>> GetJobPostsByJobType(string jobType, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new JobPostListQueryDto { JobType = jobType, Page = page, PageSize = pageSize };
        var jobPostsWithSteps = await _jobPostOrchestrator.GetJobPostsWithStepsPagedAsync(query);
        return Ok(jobPostsWithSteps);
    }

    // Create a new job post (optionally with steps)
    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobPostDto>> CreateJobPost(JobPostDto jobPostDto)
    {
        var jobPost = await _jobPostOrchestrator.CreateJobPostWithStepsAsync(jobPostDto);
        return Created("", jobPost);
    }

    // Update a job post (creates new version)
    [HttpPut("{name}/{version}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobPostDto>> UpdateJobPost(string name, int version, JobPostDto jobPostDto)
    {
        jobPostDto.Name = name;
        jobPostDto.Version = version;
        
        var jobPost = await _jobPostOrchestrator.UpdateJobPostWithStepsAsync(jobPostDto);
        return Ok(jobPost);
    }

    // Delete a job post by name and version
    [HttpDelete("{name}/{version}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> DeleteJobPost(string name, int version)
    {
        var result = await _jobPostOrchestrator.DeleteJobPostWithStepsAsync(name, version);
        return result.IsSuccess ? NoContent() : NotFound();
    }

    // Duplicate a job post into a fresh v1 with a new name/title (steps are re-assigned)
    [HttpPost("{name}/{version}/duplicate")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobPostDto>> DuplicateJobPost(string name, int version, [FromBody] DuplicateJobPostRequestDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var duplicated = await _jobPostOrchestrator.DuplicateJobPostWithStepsAsync(name, version, request);
        if (duplicated == null)
            return NotFound();

        return CreatedAtAction(nameof(GetJobPost), new { name = duplicated.Name, version = duplicated.Version }, duplicated);
    }

    // Test endpoint to verify role-based authorization
    [HttpGet("test-auth")]
    public ActionResult<object> TestAuthorization()
    {
        var user = User;
        var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var roles = user.Claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value).ToList();
        
        return Ok(new
        {
            Message = "Authorization successful",
            User = user.Identity?.Name,
            Roles = roles,
            AllClaims = claims
        });
    }

    // NEW CANDIDATE-RELATED ENDPOINTS

    /// <summary>
    /// Get filtered job posts with candidate counts (like job/filtered but with candidate counts)
    /// </summary>
    [HttpGet("filtered/with-candidate-counts")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<PagedResult<JobPostWithCandidatesDto>>> GetFilteredJobPostsWithCandidateCounts([FromQuery] JobPostListQueryDto query)
    {
        var result = await _jobPostService.GetListWithCandidateCountsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Get job post with candidates (for job post view)
    /// </summary>
    [HttpGet("{name}/{version}/with-candidates")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobPostWithCandidatesDto>> GetJobPostWithCandidates(string name, int version)
    {
        var result = await _jobPostService.GetByIdWithCandidatesAsync(name, version);
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Get candidates for a specific job post
    /// </summary>
    [HttpGet("{name}/{version}/candidates")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<JobPostCandidateDto>>> GetJobPostCandidates(string name, int version)
    {
        var candidates = await _candidateService.GetJobPostCandidatesAsync(name, version);
        return Ok(candidates);
    }

    /// <summary>
    /// Get all candidates for admin view
    /// </summary>
    [HttpGet("candidates")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<JobPostCandidateDto>>> GetAllCandidates()
    {
        var candidates = await _candidateService.GetAllCandidatesAsync();
        return Ok(candidates);
    }
}
