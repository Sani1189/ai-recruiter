using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.VolunteerExtracurricular.Dto;
using Recruiter.Application.VolunteerExtracurricular.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class VolunteerExtracurricularController(
    IVolunteerExtracurricularService volunteerExtracurricularService,
    ICurrentUserService currentUserService
) : ControllerBase
{
    private readonly IVolunteerExtracurricularService _volunteerExtracurricularService = volunteerExtracurricularService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get volunteer/extracurricular by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<VolunteerExtracurricularDto>> GetVolunteerExtracurricularById(Guid id)
    {
        var volunteerExtracurricular = await _volunteerExtracurricularService.GetByIdAsync(id);
        if (volunteerExtracurricular == null)
            return NotFound();
        return Ok(volunteerExtracurricular);
    }

    // Get all volunteer/extracurricular activities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VolunteerExtracurricularDto>>> GetAllVolunteerExtracurricular()
    {
        var volunteerExtracurricular = await _volunteerExtracurricularService.GetAllAsync();
        return Ok(volunteerExtracurricular);
    }

    // Get volunteer/extracurricular by user profile ID
    [HttpGet("user-profile")]
    public async Task<ActionResult<IEnumerable<VolunteerExtracurricularDto>>> GetVolunteerExtracurricularByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _volunteerExtracurricularService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Create volunteer/extracurricular
    [HttpPost]
    public async Task<ActionResult<VolunteerExtracurricularDto>> CreateVolunteerExtracurricular([FromBody] VolunteerExtracurricularDto volunteerExtracurricularDto)
    {
        if (volunteerExtracurricularDto == null)
            return BadRequest("Volunteer/Extracurricular data is required");

        var createdVolunteerExtracurricular = await _volunteerExtracurricularService.CreateAsync(volunteerExtracurricularDto);
        return CreatedAtAction(nameof(GetVolunteerExtracurricularById), new { id = createdVolunteerExtracurricular.Id }, createdVolunteerExtracurricular);
    }

    // Update volunteer/extracurricular
    [HttpPut("{id}")]
    public async Task<ActionResult<VolunteerExtracurricularDto>> UpdateVolunteerExtracurricular(Guid id, [FromBody] VolunteerExtracurricularDto volunteerExtracurricularDto)
    {
        if (volunteerExtracurricularDto == null)
            return BadRequest("Volunteer/Extracurricular data is required");

        volunteerExtracurricularDto.Id = id;

        var updatedVolunteerExtracurricular = await _volunteerExtracurricularService.UpdateAsync(volunteerExtracurricularDto);
        return Ok(updatedVolunteerExtracurricular);
    }

    // Delete volunteer/extracurricular
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteVolunteerExtracurricular(Guid id)
    {
        var exists = await _volunteerExtracurricularService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _volunteerExtracurricularService.DeleteAsync(id);
        return NoContent();
    }
}
