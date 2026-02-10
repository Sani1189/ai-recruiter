using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.Skill.Dto;
using Recruiter.Application.Skill.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class SkillController(ISkillService skillService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly ISkillService _skillService = skillService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get all skills
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetAllSkills()
    {
        var skills = await _skillService.GetAllAsync();
        return Ok(skills);
    }

    // Get skills by user profile ID (for current user)
    [HttpGet("user-profile")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkillsByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _skillService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Get skills by user profile ID (admin endpoint for viewing candidate data)
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkillsByUserProfileIdForAdmin(Guid userProfileId)
    {
        var result = await _skillService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Get skill by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<SkillDto>> GetSkillById(Guid id)
    {
        var skill = await _skillService.GetByIdAsync(id);
        if (skill == null)
            return NotFound();
        return Ok(skill);
    }

    // Create skill
    [HttpPost]
    public async Task<ActionResult<SkillDto>> CreateSkill([FromBody] SkillDto skillDto)
    {
        if (skillDto == null)
            return BadRequest("Skill data is required");

        var createdSkill = await _skillService.CreateAsync(skillDto);
        return CreatedAtAction(nameof(GetSkillById), new { id = createdSkill.Id }, createdSkill);
    }

    // Update skill
    [HttpPut("{id}")]
    public async Task<ActionResult<SkillDto>> UpdateSkill(Guid id, [FromBody] SkillDto skillDto)
    {
        if (skillDto == null)
            return BadRequest("Skill data is required");

        skillDto.Id = id;

        var updatedSkill = await _skillService.UpdateAsync(skillDto);
        return Ok(updatedSkill);
    }

    // Delete skill
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSkill(Guid id)
    {
        var exists = await _skillService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _skillService.DeleteAsync(id);
        return NoContent();
    }
}
