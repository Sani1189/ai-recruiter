using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
public class JobStepAssignmentController : ControllerBase
{
    private readonly IJobPostStepAssignmentService _assignmentService;

    public JobStepAssignmentController(IJobPostStepAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    // Get all step assignments for a job post
    [HttpGet("job/{jobPostName}/{jobPostVersion}")]
    public async Task<ActionResult<IEnumerable<JobPostStepAssignmentDto>>> GetJobPostStepAssignments(string jobPostName, int jobPostVersion)
    {
        var assignments = await _assignmentService.GetByJobPostAsync(jobPostName, jobPostVersion);
        return Ok(assignments);
    }

    // Assign step to job post
    [HttpPost("{jobPostName}/{jobPostVersion}")]
    public async Task<ActionResult<JobPostStepAssignmentDto>> AssignStep(string jobPostName, int jobPostVersion, [FromBody] AssignStepRequestDto request)
    {
        var assignmentDto = new JobPostStepAssignmentDto
        {
            JobPostName = jobPostName,
            JobPostVersion = jobPostVersion,
            StepNumber = request.StepNumber,
            Status = request.Status ?? "pending",
            StepDetails = request.StepDetails
        };
        
        var assignment = await _assignmentService.AssignStepAsync(assignmentDto);
        return Ok(assignment);
    }

    // Assign multiple steps to job post (bulk assignment)
    [HttpPost("{jobPostName}/{jobPostVersion}/bulk")]
    public async Task<ActionResult<IEnumerable<JobPostStepAssignmentDto>>> AssignSteps(string jobPostName, int jobPostVersion, [FromBody] List<AssignStepRequestDto> requests)
    {
        var assignments = new List<JobPostStepAssignmentDto>();
        
        foreach (var request in requests)
        {
            var assignmentDto = new JobPostStepAssignmentDto
            {
                JobPostName = jobPostName,
                JobPostVersion = jobPostVersion,
                StepNumber = request.StepNumber,
                Status = request.Status ?? "pending",
                StepDetails = request.StepDetails
            };
            
            var assignment = await _assignmentService.AssignStepAsync(assignmentDto);
            assignments.Add(assignment);
        }
        
        return Ok(assignments);
    }

    // Remove step assignment from job post
    [HttpDelete("{jobPostName}/{jobPostVersion}/{stepName}/{stepVersion}")]
    public async Task<ActionResult> UnassignStep(string jobPostName, int jobPostVersion, string stepName, int stepVersion)
    {
        await _assignmentService.UnassignStepAsync(jobPostName, jobPostVersion, stepName, stepVersion);
        return NoContent();
    }

    // Update assignment status
    [HttpPut("{jobPostName}/{jobPostVersion}/{stepName}/{stepVersion}/status")]
    public async Task<ActionResult> UpdateAssignmentStatus(string jobPostName, int jobPostVersion, string stepName, int stepVersion, [FromBody] string status)
    {
        await _assignmentService.UpdateAssignmentStatusAsync(jobPostName, jobPostVersion, stepName, stepVersion, status);
        return NoContent();
    }
}
