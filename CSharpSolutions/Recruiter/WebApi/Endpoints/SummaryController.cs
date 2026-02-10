using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.Summary.Dto;
using Recruiter.Application.Summary.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class SummaryController(ISummaryService summaryService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly ISummaryService _summaryService = summaryService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get all summaries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SummaryDto>>> GetAllSummaries()
    {
        var summaries = await _summaryService.GetAllAsync();
        return Ok(summaries);
    }

    // Get summaries by user profile ID (for current user)
    [HttpGet("user-profile")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<IEnumerable<SummaryDto>>> GetSummariesByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _summaryService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get summaries by user profile ID (admin endpoint for viewing candidate data)
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<SummaryDto>>> GetSummariesByUserProfileIdForAdmin(Guid userProfileId)
    {
        var result = await _summaryService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get summary by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<SummaryDto>> GetSummaryById(Guid id)
    {
        var summary = await _summaryService.GetByIdAsync(id);
        if (summary == null)
            return NotFound();
        return Ok(summary);
    }

    // Create summary
    [HttpPost]
    public async Task<ActionResult<SummaryDto>> CreateSummary([FromBody] SummaryDto summaryDto)
    {
        if (summaryDto == null)
            return BadRequest("Summary data is required");

        var createdSummary = await _summaryService.CreateAsync(summaryDto);
        return CreatedAtAction(nameof(GetSummaryById), new { id = createdSummary.Id }, createdSummary);
    }

    // Update summary
    [HttpPut("{id}")]
    public async Task<ActionResult<SummaryDto>> UpdateSummary(Guid id, [FromBody] SummaryDto summaryDto)
    {
        if (summaryDto == null)
            return BadRequest("Summary data is required");

        summaryDto.Id = id;

        var updatedSummary = await _summaryService.UpdateAsync(summaryDto);
        return Ok(updatedSummary);
    }

    // Delete summary
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSummary(Guid id)
    {
        var exists = await _summaryService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _summaryService.DeleteAsync(id);
        return NoContent();
    }
}
