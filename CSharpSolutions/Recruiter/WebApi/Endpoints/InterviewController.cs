using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.Interview.Dto;
using Recruiter.Application.Interview.Interfaces;
using Recruiter.Application.Common.Dto;

namespace Recruiter.WebApi.Endpoints;

/// <summary>
/// Controller for Interview operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InterviewController(IInterviewService interviewService) : ControllerBase
{
    private readonly IInterviewService _interviewService = interviewService;

    #region CRUD Operations

    // Get interview by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<InterviewDto>> GetInterviewById(Guid id)
    {
        var interview = await _interviewService.GetByIdAsync(id);
        if (interview == null)
            return NotFound();
        
        return Ok(interview);
    }

    // Get all interviews
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetAllInterviews()
    {
        var interviews = await _interviewService.GetAllAsync();
        return Ok(interviews);
    }

    // Create a new interview
    [HttpPost]
    public async Task<ActionResult<InterviewDto>> CreateInterview(InterviewDto interviewDto)
    {
        var interview = await _interviewService.CreateAsync(interviewDto);
        return CreatedAtAction(nameof(GetInterviewById), new { id = interview.Id }, interview);
    }

    // Update an interview
    [HttpPut("{id}")]
    public async Task<ActionResult<InterviewDto>> UpdateInterview(Guid id, InterviewDto interviewDto)
    {
        if (interviewDto == null)
            return BadRequest("Interview data is required");

        // Set the ID from the route parameter
        interviewDto.Id = id;

        var interview = await _interviewService.UpdateAsync(interviewDto);
        return Ok(interview);
    }

    // Delete an interview
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteInterview(Guid id)
    {
        await _interviewService.DeleteAsync(id);
        return NoContent();
    }

    // Check if an interview exists
    [HttpHead("{id}")]
    public async Task<ActionResult> InterviewExists(Guid id)
    {
        var exists = await _interviewService.ExistsAsync(id);
        return exists ? Ok() : NotFound();
    }

    #endregion

    #region Query Operations

    // Get interviews by job application step ID
    [HttpGet("by-job-application-step/{jobApplicationStepId}")]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetInterviewsByJobApplicationStepId(Guid jobApplicationStepId)
    {
        var result = await _interviewService.GetByJobApplicationStepIdAsync(jobApplicationStepId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get interviews by configuration
    [HttpGet("by-configuration")]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetInterviewsByConfiguration([FromQuery] string configName, [FromQuery] int configVersion)
    {
        var result = await _interviewService.GetByConfigurationAsync(configName, configVersion);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get filtered interviews with pagination
    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<InterviewDto>>> GetFilteredInterviews([FromQuery] InterviewListQueryDto query)
    {
        var result = await _interviewService.GetFilteredInterviewsAsync(query);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    #endregion

    #region Nested Operations - Application Context

    // Get interviews for a specific job application
    [HttpGet("applications/{appId}/interviews")]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetInterviewsByApplication(Guid appId)
    {
        var result = await _interviewService.GetByJobApplicationStepIdAsync(appId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Create interview for a specific job application
    [HttpPost("applications/{appId}/interviews")]
    public async Task<ActionResult<InterviewDto>> CreateInterviewForApplication(Guid appId, InterviewDto interviewDto)
    {
        if (interviewDto == null)
            return BadRequest("Interview data is required");

        // Set the JobApplicationStepId from the route parameter
        interviewDto.JobApplicationStepId = appId;

        var interview = await _interviewService.CreateAsync(interviewDto);
        return CreatedAtAction(nameof(GetInterviewById), new { id = interview.Id }, interview);
    }

    #endregion

    #region Nested Operations - Step Context

    // Get interviews for a specific job application step
    [HttpGet("applications/{appId}/steps/{stepId}/interviews")]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetInterviewsByStep(Guid appId, Guid stepId)
    {
        var result = await _interviewService.GetByJobApplicationStepIdAsync(stepId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Schedule interview for a specific job application step
    [HttpPost("applications/{appId}/steps/{stepId}/interviews")]
    public async Task<ActionResult<InterviewDto>> ScheduleInterviewForStep(Guid appId, Guid stepId, CreateInterviewDto createDto)
    {
        // Ignore body; create interview using server-side resolution of configuration/prompts
        var result = await _interviewService.CreateForStepAsync(stepId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return CreatedAtAction(nameof(GetInterviewById), new { id = result.Value.Id }, result.Value);
    }

    // Get a specific interview for a job application step
    [HttpGet("applications/{appId}/steps/{stepId}/interviews/{intId}")]
    public async Task<ActionResult<InterviewDto>> GetInterviewByStep(Guid appId, Guid stepId, Guid intId)
    {
        var interview = await _interviewService.GetByIdAsync(intId);
        if (interview == null)
            return NotFound();

        // Verify the interview belongs to the step
        if (interview.JobApplicationStepId != stepId)
            return BadRequest("Interview does not belong to this step");

        return Ok(interview);
    }

    // Update a specific interview for a job application step
    [HttpPut("applications/{appId}/steps/{stepId}/interviews/{intId}")]
    public async Task<ActionResult<InterviewDto>> UpdateInterviewByStep(Guid appId, Guid stepId, Guid intId, InterviewDto interviewDto)
    {
        if (interviewDto == null)
            return BadRequest("Interview data is required");

        // Verify the interview belongs to the step
        var existingInterview = await _interviewService.GetByIdAsync(intId);
        if (existingInterview == null)
            return NotFound();

        if (existingInterview.JobApplicationStepId != stepId)
            return BadRequest("Interview does not belong to this step");

        // Set the IDs from the route parameters
        interviewDto.Id = intId;
        interviewDto.JobApplicationStepId = stepId;

        var interview = await _interviewService.UpdateAsync(interviewDto);
        return Ok(interview);
    }

    // Delete a specific interview for a job application step
    [HttpDelete("applications/{appId}/steps/{stepId}/interviews/{intId}")]
    public async Task<ActionResult> DeleteInterviewByStep(Guid appId, Guid stepId, Guid intId)
    {
        // Verify the interview belongs to the step
        var existingInterview = await _interviewService.GetByIdAsync(intId);
        if (existingInterview == null)
            return NotFound();

        if (existingInterview.JobApplicationStepId != stepId)
            return BadRequest("Interview does not belong to this step");

        await _interviewService.DeleteAsync(intId);
        return NoContent();
    }

    // Complete an interview
    [HttpPut("applications/{appId}/steps/{stepId}/interviews/{intId}/complete")]
    public async Task<ActionResult<InterviewDto>> CompleteInterview(Guid appId, Guid stepId, Guid intId, [FromBody] CompleteInterviewDto completeDto)
    {
        if (completeDto == null)
            return BadRequest("Completion data is required");

        // Verify the interview belongs to the step
        var existingInterview = await _interviewService.GetByIdAsync(intId);
        if (existingInterview == null)
            return NotFound();

        if (existingInterview.JobApplicationStepId != stepId)
            return BadRequest("Interview does not belong to this step");

        var result = await _interviewService.CompleteInterviewAsync(intId, completeDto);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    #endregion
}
