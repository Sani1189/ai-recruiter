using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.Experience.Dto;
using Recruiter.Application.Experience.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class ExperienceController(IExperienceService experienceService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IExperienceService _experienceService = experienceService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get all experiences
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExperienceDto>>> GetAllExperiences()
    {
        var experiences = await _experienceService.GetAllAsync();
        return Ok(experiences);
    }

    // Get experiences by user profile ID (for current user)
    [HttpGet("user-profile")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<IEnumerable<ExperienceDto>>> GetExperiencesByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _experienceService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get experiences by user profile ID (admin endpoint for viewing candidate data)
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<ExperienceDto>>> GetExperiencesByUserProfileIdForAdmin(Guid userProfileId)
    {
        var result = await _experienceService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get experience by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ExperienceDto>> GetExperienceById(Guid id)
    {
        var experience = await _experienceService.GetByIdAsync(id);
        if (experience == null)
            return NotFound();
        return Ok(experience);
    }

    // Create experience
    [HttpPost]
    public async Task<ActionResult<ExperienceDto>> CreateExperience([FromBody] ExperienceDto experienceDto)
    {
        if (experienceDto == null)
            return BadRequest("Experience data is required");

        var createdExperience = await _experienceService.CreateAsync(experienceDto);
        return CreatedAtAction(nameof(GetExperienceById), new { id = createdExperience.Id }, createdExperience);
    }

    // Update experience
    [HttpPut("{id}")]
    public async Task<ActionResult<ExperienceDto>> UpdateExperience(Guid id, [FromBody] ExperienceDto experienceDto)
    {
        if (experienceDto == null)
            return BadRequest("Experience data is required");

        experienceDto.Id = id;

        var updatedExperience = await _experienceService.UpdateAsync(experienceDto);
        return Ok(updatedExperience);
    }

    // Delete experience
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExperience(Guid id)
    {
        var exists = await _experienceService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _experienceService.DeleteAsync(id);
        return NoContent();
    }
}
