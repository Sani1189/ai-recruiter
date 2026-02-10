using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.JobApplicationStepFiles.Dto;
using Recruiter.Application.JobApplicationStepFiles.Interfaces;
using Recruiter.Application.Common.Dto;
using Microsoft.AspNetCore.Http;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
public class JobApplicationStepFilesController(IJobApplicationStepFilesService jobApplicationStepFilesService) : ControllerBase
{
    private readonly IJobApplicationStepFilesService _jobApplicationStepFilesService = jobApplicationStepFilesService;

    #region CRUD Operations

    // Get all job application step files
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobApplicationStepFilesDto>>> GetAllJobApplicationStepFiles()
    {
        var jobApplicationStepFiles = await _jobApplicationStepFilesService.GetAllAsync();
        return Ok(jobApplicationStepFiles);
    }

    // Get job application step file by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplicationStepFilesDto>> GetJobApplicationStepFile(Guid id)
    {
        var result = await _jobApplicationStepFilesService.GetByIdAsync(id, CancellationToken.None);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Create new job application step file
    [HttpPost]
    public async Task<ActionResult<JobApplicationStepFilesDto>> CreateJobApplicationStepFile([FromBody] JobApplicationStepFilesDto jobApplicationStepFileDto)
    {
        if (jobApplicationStepFileDto == null)
            return BadRequest("Job application step file data is required");

        var jobApplicationStepFile = await _jobApplicationStepFilesService.CreateAsync(jobApplicationStepFileDto);
        return CreatedAtAction(nameof(GetJobApplicationStepFile), new { id = jobApplicationStepFile.Id }, jobApplicationStepFile);
    }

    // Update job application step file
    [HttpPut("{id}")]
    public async Task<ActionResult<JobApplicationStepFilesDto>> UpdateJobApplicationStepFile(Guid id, [FromBody] JobApplicationStepFilesDto jobApplicationStepFileDto)
    {
        if (jobApplicationStepFileDto == null)
            return BadRequest("Job application step file data is required");

        // Set the ID from the route parameter
        jobApplicationStepFileDto.Id = id;

        var jobApplicationStepFile = await _jobApplicationStepFilesService.UpdateAsync(jobApplicationStepFileDto);
        return Ok(jobApplicationStepFile);
    }

    // Delete job application step file
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJobApplicationStepFile(Guid id)
    {
        await _jobApplicationStepFilesService.DeleteAsync(id);
        return NoContent();
    }

    // Check if job application step file exists
    [HttpHead("{id}")]
    public async Task<ActionResult> JobApplicationStepFileExists(Guid id)
    {
        var exists = await _jobApplicationStepFilesService.ExistsAsync(id);
        return exists ? Ok() : NotFound();
    }

    #endregion

    #region Query Operations

    // Get job application step files by file ID
    [HttpGet("by-file/{fileId}")]
    public async Task<ActionResult<IEnumerable<JobApplicationStepFilesDto>>> GetJobApplicationStepFilesByFileId(Guid fileId)
    {
        var result = await _jobApplicationStepFilesService.GetByFileIdAsync(fileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get job application step files by job application step ID
    [HttpGet("by-job-application-step/{jobApplicationStepId}")]
    public async Task<ActionResult<IEnumerable<JobApplicationStepFilesDto>>> GetJobApplicationStepFilesByJobApplicationStepId(Guid jobApplicationStepId)
    {
        var result = await _jobApplicationStepFilesService.GetByJobApplicationStepIdAsync(jobApplicationStepId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Check if file is attached to step
    [HttpGet("is-attached")]
    public async Task<ActionResult<bool>> IsFileAttachedToStep([FromQuery] Guid fileId, [FromQuery] Guid jobApplicationStepId)
    {
        var result = await _jobApplicationStepFilesService.IsFileAttachedToStepAsync(fileId, jobApplicationStepId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get filtered job application step files with pagination
    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<JobApplicationStepFilesDto>>> GetFilteredJobApplicationStepFiles([FromQuery] JobApplicationStepFilesListQueryDto query)
    {
        var result = await _jobApplicationStepFilesService.GetFilteredJobApplicationStepFilesAsync(query);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    #endregion
}
