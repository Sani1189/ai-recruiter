using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.Candidate.Dto;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Application.Common.Dto;
using Recruiter.WebApi.Attributes;

namespace Recruiter.WebApi.Endpoints;

// Candidate API Controller - handles all candidate operations (CRUD + Queries)
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Admin")]
public class CandidateController(ICandidateService candidateService) : ControllerBase
{
    private readonly ICandidateService _candidateService = candidateService;

    #region CRUD Operations

    // Get candidate by ID with UserProfile details
    [HttpGet("{id}")]
    public async Task<ActionResult<CandidateDto>> GetCandidateById(Guid id)
    {
        var result = await _candidateService.GetCandidateDetailsById(id);
        if (!result.IsSuccess)
            return NotFound();
        return Ok(result.Value);
    }

    // Get all candidates with UserProfile information
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidateDto>>> GetAllCandidates()
    {
        var candidates = await _candidateService.GetAllWithUserProfileAsync();
        return Ok(candidates);
    }

    // Check if candidate exists
    [HttpHead("{id}")]
    public async Task<ActionResult> CandidateExists(Guid id)
    {
        var exists = await _candidateService.ExistsAsync(id);
        return exists ? Ok() : NotFound();
    }

    #endregion

    #region Query Operations

    // Get candidate by user ID with UserProfile details
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<CandidateDto>> GetCandidateByUserId(Guid userId)
    {
        var result = await _candidateService.GetByUserIdWithUserProfileAsync(userId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get recent candidates with UserProfile information
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<CandidateDto>>> GetRecentCandidates([FromQuery] int days = 30)
    {
        var result = await _candidateService.GetRecentCandidatesWithUserProfileAsync(days);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get filtered candidates with pagination and UserProfile information
    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<CandidateDto>>> GetFilteredCandidates([FromQuery] CandidateListQueryDto query)
    {
        var result = await _candidateService.GetFilteredCandidatesWithUserProfileAsync(query);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }


    #endregion
}
