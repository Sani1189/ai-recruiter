using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.KeyStrength.Dto;
using Recruiter.Application.KeyStrength.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class KeyStrengthController(IKeyStrengthService keyStrengthService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IKeyStrengthService _keyStrengthService = keyStrengthService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get all key strengths
    [HttpGet]
    public async Task<ActionResult<IEnumerable<KeyStrengthDto>>> GetAllKeyStrengths()
    {
        var keyStrengths = await _keyStrengthService.GetAllAsync();
        return Ok(keyStrengths);
    }

    // Get key strengths by user profile ID (for current user)
    [HttpGet("user-profile")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<IEnumerable<KeyStrengthDto>>> GetKeyStrengthsByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _keyStrengthService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get key strengths by user profile ID (admin endpoint for viewing candidate data)
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<KeyStrengthDto>>> GetKeyStrengthsByUserProfileIdForAdmin(Guid userProfileId)
    {
        var result = await _keyStrengthService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get key strength by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<KeyStrengthDto>> GetKeyStrengthById(Guid id)
    {
        var keyStrength = await _keyStrengthService.GetByIdAsync(id);
        if (keyStrength == null)
            return NotFound();
        return Ok(keyStrength);
    }

    // Create key strength
    [HttpPost]
    public async Task<ActionResult<KeyStrengthDto>> CreateKeyStrength([FromBody] KeyStrengthDto keyStrengthDto)
    {
        if (keyStrengthDto == null)
            return BadRequest("Key strength data is required");

        var createdKeyStrength = await _keyStrengthService.CreateAsync(keyStrengthDto);
        return CreatedAtAction(nameof(GetKeyStrengthById), new { id = createdKeyStrength.Id }, createdKeyStrength);
    }

    // Update key strength
    [HttpPut("{id}")]
    public async Task<ActionResult<KeyStrengthDto>> UpdateKeyStrength(Guid id, [FromBody] KeyStrengthDto keyStrengthDto)
    {
        if (keyStrengthDto == null)
            return BadRequest("Key strength data is required");

        keyStrengthDto.Id = id;

        var updatedKeyStrength = await _keyStrengthService.UpdateAsync(keyStrengthDto);
        return Ok(updatedKeyStrength);
    }

    // Delete key strength
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteKeyStrength(Guid id)
    {
        var exists = await _keyStrengthService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _keyStrengthService.DeleteAsync(id);
        return NoContent();
    }
}
