using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.Common.Dto;
using Recruiter.WebApi.Attributes;
using Ardalis.Result;

namespace Recruiter.WebApi.Endpoints;

// JobApplication API Controller - handles admin job application operations (CRUD + Queries)
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Admin")]
public class JobApplicationController(IJobApplicationService jobApplicationService, ILogger<JobApplicationController> logger) : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService = jobApplicationService;
    private readonly ILogger<JobApplicationController> _logger = logger;


    #region Admin Endpoints

    // Get all job applications with filtering and pagination (for admins)
    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<Recruiter.Application.Common.Dto.PagedResult<JobApplicationDto>>> GetAllJobApplications([FromQuery] JobApplicationListQueryDto query)
    {
        var result = await _jobApplicationService.GetFilteredJobApplicationsAsync(query);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Get job application by ID (for admins)
    [HttpGet("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobApplicationDto>> GetJobApplicationById(Guid id)
    {
        var result = await _jobApplicationService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return result.Status == ResultStatus.NotFound ? NotFound() : BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Get job applications by candidate ID (for admins)
    [HttpGet("candidate/{candidateId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<JobApplicationDto>>> GetJobApplicationsByCandidate(Guid candidateId)
    {
        var result = await _jobApplicationService.GetByCandidateIdAsync(candidateId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Get job applications by job post (for admins)
    [HttpGet("jobpost/{jobPostName}/{jobPostVersion}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<JobApplicationDto>>> GetJobApplicationsByJobPost(string jobPostName, int jobPostVersion)
    {
        var result = await _jobApplicationService.GetByJobPostAsync(jobPostName, jobPostVersion);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Get job application with steps and interviews for audio player (for admins/recruiters)
    [HttpGet("jobpost/{jobPostName}/{jobPostVersion}/candidate/{candidateId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobApplicationWithStepsAndInterviewsDto>> GetJobApplicationWithInterviews(string jobPostName, int jobPostVersion, Guid candidateId)
    {
        var result = await _jobApplicationService.GetJobApplicationWithStepsAndInterviewsAsync(jobPostName, jobPostVersion, candidateId);
        if (!result.IsSuccess)
            return result.Status == ResultStatus.NotFound ? NotFound() : BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Get completed job applications (for admins)
    [HttpGet("completed")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<JobApplicationDto>>> GetCompletedJobApplications()
    {
        var result = await _jobApplicationService.GetCompletedJobApplicationsAsync();
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Update application status (for admins)
    [HttpPut("{id}/status")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobApplicationDto>> UpdateApplicationStatus(Guid id, [FromBody] UpdateApplicationStatusDto statusDto)
    {
        if (statusDto == null)
            return BadRequest("Status data is required");

        var result = await _jobApplicationService.UpdateApplicationStatusAsync(id, statusDto.Status);
        if (!result.IsSuccess)
            return result.Status == ResultStatus.NotFound ? NotFound() : BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Promote candidate to next step (for admins/recruiters)
    [HttpPut("{id}/promote-step")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobApplicationDto>> PromoteToNextStep(Guid id, [FromBody] PromoteStepDto promoteDto)
    {
        if (promoteDto == null)
            return BadRequest("Promotion data is required");

        _logger.LogInformation($"[DEBUG] PromoteToNextStep called with id={id}");
        _logger.LogInformation($"[DEBUG] PromoteDto: JobPostName={promoteDto.JobPostName}, JobPostVersion={promoteDto.JobPostVersion}, CurrentStep={promoteDto.CurrentStep}, NextStep={promoteDto.NextStep}");

        var result = await _jobApplicationService.PromoteToNextStepAsync(id, promoteDto);
        
        if (!result.IsSuccess)
        {
            _logger.LogError($"[DEBUG] Promotion failed: {string.Join(", ", result.ValidationErrors)}");
            return result.Status == ResultStatus.NotFound ? NotFound() : BadRequest(result.ValidationErrors);
        }
        
        _logger.LogInformation($"[DEBUG] Promotion successful for application {id}");
        return Ok(result.Value);
    }

    #endregion
}
