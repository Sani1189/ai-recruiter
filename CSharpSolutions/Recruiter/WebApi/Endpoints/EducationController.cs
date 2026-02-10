using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.Education.Dto;
using Recruiter.Application.Education.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class EducationController(IEducationService educationService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IEducationService _educationService = educationService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get all educations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EducationDto>>> GetAllEducations()
    {
        var educations = await _educationService.GetAllAsync();
        return Ok(educations);
    }

    // Get educations by user profile ID (for current user)
    [HttpGet("user-profile")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<IEnumerable<EducationDto>>> GetEducationsByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _educationService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get educations by user profile ID (admin endpoint for viewing candidate data)
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<EducationDto>>> GetEducationsByUserProfileIdForAdmin(Guid userProfileId)
    {
        var result = await _educationService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get education by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<EducationDto>> GetEducationById(Guid id)
    {
        var education = await _educationService.GetByIdAsync(id);
        if (education == null)
            return NotFound();
        return Ok(education);
    }

    // Create education
    [HttpPost]
    public async Task<ActionResult<EducationDto>> CreateEducation([FromBody] EducationDto educationDto)
    {
        if (educationDto == null)
            return BadRequest("Education data is required");

        var createdEducation = await _educationService.CreateAsync(educationDto);
        return CreatedAtAction(nameof(GetEducationById), new { id = createdEducation.Id }, createdEducation);
    }

    // Update education
    [HttpPut("{id}")]
    public async Task<ActionResult<EducationDto>> UpdateEducation(Guid id, [FromBody] EducationDto educationDto)
    {
        if (educationDto == null)
            return BadRequest("Education data is required");

        educationDto.Id = id;

        var updatedEducation = await _educationService.UpdateAsync(educationDto);
        return Ok(updatedEducation);
    }

    // Delete education
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEducation(Guid id)
    {
        var exists = await _educationService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _educationService.DeleteAsync(id);
        return NoContent();
    }
}
