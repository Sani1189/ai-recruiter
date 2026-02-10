using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
public class JobStepController : ControllerBase
{
    private readonly IJobPostStepService _jobPostStepService;

    public JobStepController(IJobPostStepService jobPostStepService)
    {
        _jobPostStepService = jobPostStepService;
    }

    // Get step template by name and version
    [HttpGet("{name}/{version}")]
    public async Task<ActionResult<JobPostStepDto>> GetJobPostStep(string name, int version)
    {
        var step = await _jobPostStepService.GetByIdAsync(name, version);
        if (step == null)
            return NotFound();
        
        return Ok(step);
    }

    // Get latest version of step template by name
    [HttpGet("{name}/latest")]
    public async Task<ActionResult<JobPostStepDto>> GetLatestJobPostStep(string name)
    {
        var step = await _jobPostStepService.GetLatestVersionAsync(name);
        if (step == null)
            return NotFound();
        
        return Ok(step);
    }

    // Get step templates (simple list - supports filtering but no pagination)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobPostStepDto>>> GetJobPostSteps([FromQuery] JobPostStepQueryDto query)
    {
        // Set pagination to get all results
        query.Page = 1;
        query.PageSize = int.MaxValue;
        
        var steps = await _jobPostStepService.GetListAsync(query);
        return Ok(steps.Items);
    }

    // Get filtered step templates with pagination and filters
    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<JobPostStepDto>>> GetFilteredJobPostSteps([FromQuery] JobPostStepQueryDto query)
    {
        var steps = await _jobPostStepService.GetListAsync(query);
        return Ok(steps);
    }

    // Get dropdown list for selecting existing steps
    [HttpGet("dropdown")]
    public async Task<ActionResult<IEnumerable<JobPostStepDto>>> GetDropdownList()
    {
        var steps = await _jobPostStepService.GetDropdownListAsync();
        return Ok(steps);
    }

    [HttpGet("{name}/versions")]
    public async Task<ActionResult<IEnumerable<JobStepVersionDto>>> GetAllVersions(string name)
    {
        var versions = await _jobPostStepService.GetAllVersionsAsync(name);
        return Ok(versions);
    }

    // Create a new step template
    [HttpPost]
    public async Task<ActionResult<JobPostStepDto>> CreateJobPostStep(JobPostStepDto jobPostStepDto)
    {
        var step = await _jobPostStepService.CreateAsync(jobPostStepDto);
        return CreatedAtAction(nameof(GetJobPostStep), 
            new { name = step.Name, version = step.Version }, step);
    }

    // Update a step template (creates new version)
    [HttpPut("{name}/{version}")]
    public async Task<ActionResult<JobPostStepDto>> UpdateJobPostStep(string name, int version, JobPostStepDto jobPostStepDto)
    {
        jobPostStepDto.Name = name;
        jobPostStepDto.Version = version;
        
        var step = await _jobPostStepService.UpdateAsync(jobPostStepDto);
        return Ok(step);
    }

    // Duplicate a step template into a fresh v1 with a new name (optionally override display title)
    [HttpPost("{name}/{version}/duplicate")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<JobPostStepDto>> DuplicateJobPostStep(string name, int version, [FromBody] DuplicateJobPostStepRequestDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var duplicated = await _jobPostStepService.DuplicateAsync(name, version, request);
        if (duplicated == null)
            return NotFound();

        return CreatedAtAction(nameof(GetJobPostStep), new { name = duplicated.Name, version = duplicated.Version }, duplicated);
    }

    // Delete a step template
    [HttpDelete("{name}/{version}")]
    public async Task<ActionResult> DeleteJobPostStep(string name, int version)
    {
        var exists = await _jobPostStepService.ExistsAsync(name, version);
        if (!exists)
            return NotFound();

        await _jobPostStepService.DeleteAsync(name, version);
        return NoContent();
    }
}
