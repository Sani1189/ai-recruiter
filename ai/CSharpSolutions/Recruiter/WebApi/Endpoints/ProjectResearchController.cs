using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.ProjectResearch.Dto;
using Recruiter.Application.ProjectResearch.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class ProjectResearchController(IProjectResearchService projectResearchService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IProjectResearchService _projectResearchService = projectResearchService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get all projects/research
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResearchDto>>> GetAllProjectsResearch()
    {
        var projectsResearch = await _projectResearchService.GetAllAsync();
        return Ok(projectsResearch);
    }

    // Get projects/research by user profile ID (for current user)
    [HttpGet("user-profile")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<IEnumerable<ProjectResearchDto>>> GetProjectsResearchByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _projectResearchService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get projects/research by user profile ID (admin endpoint for viewing candidate data)
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<ProjectResearchDto>>> GetProjectsResearchByUserProfileIdForAdmin(Guid userProfileId)
    {
        var result = await _projectResearchService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get project/research by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResearchDto>> GetProjectResearchById(Guid id)
    {
        var projectResearch = await _projectResearchService.GetByIdAsync(id);
        if (projectResearch == null)
            return NotFound();
        return Ok(projectResearch);
    }

    // Create project/research
    [HttpPost]
    public async Task<ActionResult<ProjectResearchDto>> CreateProjectResearch([FromBody] ProjectResearchDto projectResearchDto)
    {
        if (projectResearchDto == null)
            return BadRequest("Project/Research data is required");

        var createdProjectResearch = await _projectResearchService.CreateAsync(projectResearchDto);
        return CreatedAtAction(nameof(GetProjectResearchById), new { id = createdProjectResearch.Id }, createdProjectResearch);
    }

    // Update project/research
    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectResearchDto>> UpdateProjectResearch(Guid id, [FromBody] ProjectResearchDto projectResearchDto)
    {
        if (projectResearchDto == null)
            return BadRequest("Project/Research data is required");

        projectResearchDto.Id = id;

        var updatedProjectResearch = await _projectResearchService.UpdateAsync(projectResearchDto);
        return Ok(updatedProjectResearch);
    }

    // Delete project/research
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProjectResearch(Guid id)
    {
        var exists = await _projectResearchService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _projectResearchService.DeleteAsync(id);
        return NoContent();
    }
}
