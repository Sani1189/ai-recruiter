using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.CvEvaluation.Dto;
using Recruiter.Application.CvEvaluation.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "AdminOrCandidate")]
public class CvEvaluationController(ICvEvaluationService cvEvaluationService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly ICvEvaluationService _cvEvaluationService = cvEvaluationService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get all CV evaluations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CvEvaluationDto>>> GetAllCvEvaluations()
    {
        var cvEvaluations = await _cvEvaluationService.GetAllAsync();
        return Ok(cvEvaluations);
    }

    // Get CV evaluations by user profile ID (for current user)
    [HttpGet("user-profile")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<IEnumerable<CvEvaluationDto>>> GetCvEvaluationsByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _cvEvaluationService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get CV evaluations by user profile ID (admin endpoint for viewing candidate data)
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<CvEvaluationDto>>> GetCvEvaluationsByUserProfileIdForAdmin(Guid userProfileId)
    {
        var result = await _cvEvaluationService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get CV evaluation by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<CvEvaluationDto>> GetCvEvaluationById(Guid id)
    {
        var cvEvaluation = await _cvEvaluationService.GetByIdAsync(id);
        if (cvEvaluation == null)
            return NotFound();
        return Ok(cvEvaluation);
    }

    // Create CV evaluation
    [HttpPost]
    public async Task<ActionResult<CvEvaluationDto>> CreateCvEvaluation([FromBody] CvEvaluationDto cvEvaluationDto)
    {
        if (cvEvaluationDto == null)
            return BadRequest("CV Evaluation data is required");

        var createdCvEvaluation = await _cvEvaluationService.CreateAsync(cvEvaluationDto);
        return CreatedAtAction(nameof(GetCvEvaluationById), new { id = createdCvEvaluation.Id }, createdCvEvaluation);
    }

    // Update CV evaluation
    [HttpPut("{id}")]
    public async Task<ActionResult<CvEvaluationDto>> UpdateCvEvaluation(Guid id, [FromBody] CvEvaluationDto cvEvaluationDto)
    {
        if (cvEvaluationDto == null)
            return BadRequest("CV Evaluation data is required");

        cvEvaluationDto.Id = id;

        var updatedCvEvaluation = await _cvEvaluationService.UpdateAsync(cvEvaluationDto);
        return Ok(updatedCvEvaluation);
    }

    // Delete CV evaluation
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCvEvaluation(Guid id)
    {
        var exists = await _cvEvaluationService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _cvEvaluationService.DeleteAsync(id);
        return NoContent();
    }
}
