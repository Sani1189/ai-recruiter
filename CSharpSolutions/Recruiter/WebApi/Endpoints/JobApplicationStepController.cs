using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Application.Common.Dto;
using Recruiter.WebApi.Attributes;
using Ardalis.Result;

namespace Recruiter.WebApi.Endpoints;

// JobApplicationStep API Controller - handles all job application step operations (CRUD + Queries)
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrCandidate")]
public class JobApplicationStepController(IJobApplicationStepService jobApplicationStepService) : ControllerBase
{
    private readonly IJobApplicationStepService _jobApplicationStepService = jobApplicationStepService;


    #region Admin Endpoints
    // Get job application steps by application ID (for admins)
    [HttpGet("application/{jobApplicationId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<JobApplicationStepDto>>> GetJobApplicationStepsByApplicationId(Guid jobApplicationId)
    {
        var result = await _jobApplicationStepService.GetByJobApplicationIdAsync(jobApplicationId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }
    #endregion
    
    #region Candidate Endpoints
    // Get my job application steps (for candidates)
    [HttpGet("my-application/{applicationId}")]
    [Authorize(Policy = "Candidate")]
    public async Task<ActionResult<List<JobApplicationStepDto>>> GetMyApplicationSteps(Guid applicationId)
    {
        var result = await _jobApplicationStepService.GetMyApplicationStepsAsync(applicationId);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.Unauthorized => Forbid("You can only view steps for your own applications"),
                _ => BadRequest(result.ValidationErrors)
            };
        }
        return Ok(result.Value);
    }

    #endregion
}