using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.AwardAchievement.Dto;
using Recruiter.Application.AwardAchievement.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrCandidate")]
public class AwardAchievementController(IAwardAchievementService awardAchievementService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IAwardAchievementService _awardAchievementService = awardAchievementService;
    private readonly ICurrentUserService _currentUserService = currentUserService;    

    // Get all awards/achievements
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AwardAchievementDto>>> GetAllAwardsAchievements()
    {
        var awardsAchievements = await _awardAchievementService.GetAllAsync();
        return Ok(awardsAchievements);
    }

    // Get awards/achievements by user profile ID
    [HttpGet("user-profile")]
    public async Task<ActionResult<IEnumerable<AwardAchievementDto>>> GetAwardsAchievementsByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _awardAchievementService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get awards/achievements by user profile ID for admin
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<AwardAchievementDto>>> GetAwardsAchievementsByUserProfileId(Guid userProfileId)
    {
        var result = await _awardAchievementService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }
    
    // Get award/achievement by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<AwardAchievementDto>> GetAwardAchievementById(Guid id)
    {
        var awardAchievement = await _awardAchievementService.GetByIdAsync(id);
        if (awardAchievement == null)
            return NotFound();
        return Ok(awardAchievement);
    }

    // Create award/achievement
    [HttpPost]
    public async Task<ActionResult<AwardAchievementDto>> CreateAwardAchievement([FromBody] AwardAchievementDto awardAchievementDto)
    {
        if (awardAchievementDto == null)
            return BadRequest("Award/Achievement data is required");

        var createdAwardAchievement = await _awardAchievementService.CreateAsync(awardAchievementDto);
        return CreatedAtAction(nameof(GetAwardAchievementById), new { id = createdAwardAchievement.Id }, createdAwardAchievement);
    }

    // Update award/achievement
    [HttpPut("{id}")]
    public async Task<ActionResult<AwardAchievementDto>> UpdateAwardAchievement(Guid id, [FromBody] AwardAchievementDto awardAchievementDto)
    {
        if (awardAchievementDto == null)
            return BadRequest("Award/Achievement data is required");

        awardAchievementDto.Id = id;

        var updatedAwardAchievement = await _awardAchievementService.UpdateAsync(awardAchievementDto);
        return Ok(updatedAwardAchievement);
    }

    // Delete award/achievement
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAwardAchievement(Guid id)
    {
        var exists = await _awardAchievementService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _awardAchievementService.DeleteAsync(id);
        return NoContent();
    }
}
