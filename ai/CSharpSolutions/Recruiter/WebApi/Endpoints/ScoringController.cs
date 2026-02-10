using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.Scoring.Dto;
using Recruiter.Application.Scoring.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class ScoringController(IScoringService scoringService) : ControllerBase
{
    private readonly IScoringService _scoringService = scoringService;

    // Get scoring by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ScoringDto>> GetScoringById(Guid id)
    {
        var scoring = await _scoringService.GetByIdAsync(id);
        if (scoring == null)
            return NotFound();
        return Ok(scoring);
    }

    // Get all scorings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScoringDto>>> GetAllScorings()
    {
        var scorings = await _scoringService.GetAllAsync();
        return Ok(scorings);
    }

    // Get scorings by CV evaluation ID
    [HttpGet("cv-evaluation/{cvEvaluationId}")]
    public async Task<ActionResult<IEnumerable<ScoringDto>>> GetScoringsByCvEvaluationId(Guid cvEvaluationId)
    {
        var result = await _scoringService.GetByCvEvaluationIdAsync(cvEvaluationId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Create scoring
    [HttpPost]
    public async Task<ActionResult<ScoringDto>> CreateScoring([FromBody] ScoringDto scoringDto)
    {
        if (scoringDto == null)
            return BadRequest("Scoring data is required");

        var createdScoring = await _scoringService.CreateAsync(scoringDto);
        return CreatedAtAction(nameof(GetScoringById), new { id = createdScoring.Id }, createdScoring);
    }

    // Update scoring
    [HttpPut("{id}")]
    public async Task<ActionResult<ScoringDto>> UpdateScoring(Guid id, [FromBody] ScoringDto scoringDto)
    {
        if (scoringDto == null)
            return BadRequest("Scoring data is required");

        scoringDto.Id = id;

        var updatedScoring = await _scoringService.UpdateAsync(scoringDto);
        return Ok(updatedScoring);
    }

    // Delete scoring
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteScoring(Guid id)
    {
        var exists = await _scoringService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _scoringService.DeleteAsync(id);
        return NoContent();
    }
}
