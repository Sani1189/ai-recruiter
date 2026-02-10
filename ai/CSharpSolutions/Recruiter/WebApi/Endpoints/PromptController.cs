using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.Prompt;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Application.Prompt.Interfaces;
using Recruiter.Application.Common.Dto;

namespace Recruiter.WebApi.Endpoints;

// Prompt API Controller - handles all prompt operations (CRUD + Queries)
[ApiController]
[Route("api/[controller]")]
public class PromptController(IPromptService promptService) : ControllerBase
{
    private readonly IPromptService _promptService = promptService;

    #region CRUD Operations

    // Get prompt by name and version
    [HttpGet("{name}/{version}")]
    public async Task<ActionResult<PromptDto>> GetPromptByNameAndVersion(string name, int version)
    {
        var prompt = await _promptService.GetByIdAsync(name, version);
        if (prompt == null)
            return NotFound();
        return Ok(prompt);
    }

    // Get all prompts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromptDto>>> GetAllPrompts()
    {
        var result = await _promptService.GetAllAsync();
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Create new prompt
    [HttpPost]
    public async Task<ActionResult<PromptDto>> CreatePrompt([FromBody] PromptDto promptDto)
    {
        if (promptDto == null)
            return BadRequest("Prompt data is required");

        var createdPrompt = await _promptService.CreateAsync(promptDto);
        return CreatedAtAction(nameof(GetPromptByNameAndVersion), 
            new { name = createdPrompt.Name, version = createdPrompt.Version }, 
            createdPrompt);
    }

    // Update prompt
    [HttpPut("{name}/{version}")]
    public async Task<ActionResult<PromptDto>> UpdatePrompt(string name, int version, [FromBody] PromptDto promptDto)
    {
        if (promptDto == null)
            return BadRequest("Prompt data is required");

        // Set the Name and Version from the route parameters
        promptDto.Name = name;
        promptDto.Version = version;

        var updatedPrompt = await _promptService.UpdateAsync(promptDto);
        return Ok(updatedPrompt);
    }

    // Duplicate prompt into a fresh v1 with a new name
    [HttpPost("{name}/{version}/duplicate")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<PromptDto>> DuplicatePrompt(string name, int version, [FromBody] DuplicatePromptRequestDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var duplicated = await _promptService.DuplicateAsync(name, version, request);
        if (duplicated == null)
            return NotFound();

        return CreatedAtAction(nameof(GetPromptByNameAndVersion),
            new { name = duplicated.Name, version = duplicated.Version },
            duplicated);
    }

    // Delete prompt
    [HttpDelete("{name}/{version}")]
    public async Task<ActionResult> DeletePrompt(string name, int version)
    {
        var result = await _promptService.DeleteAsync(name, version);

        if (!result.IsSuccess)
            return result.Status == Ardalis.Result.ResultStatus.NotFound
                ? NotFound()
                : BadRequest(result.ValidationErrors);

        return NoContent();
    }

    // Check if prompt exists
    [HttpHead("{name}/{version}")]
    public async Task<ActionResult> PromptExists(string name, int version)
    {
        var exists = await _promptService.ExistsAsync(name, version);
        return exists ? Ok() : NotFound();
    }

    #endregion

    #region Query Operations

    // Get distinct prompt categories (for dropdowns)
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _promptService.GetCategoriesAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get latest prompt by name
    [HttpGet("{name}/latest")]
    public async Task<ActionResult<PromptDto>> GetLatestPromptByName(string name)
    {
        var result = await _promptService.GetLatestByNameAsync(name);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get all versions of a prompt by name
    [HttpGet("{name}/versions")]
    public async Task<ActionResult<IEnumerable<PromptDto>>> GetAllVersions(string name)
    {
        var versions = await _promptService.GetAllVersionsAsync(name);
        return Ok(versions);
    }

    // Get prompts by category
    [HttpGet("by-category/{category}")]
    public async Task<ActionResult<IEnumerable<PromptDto>>> GetPromptsByCategory(string category)
    {
        var result = await _promptService.GetByCategoryAsync(category);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get prompts by locale
    [HttpGet("by-locale/{locale}")]
    public async Task<ActionResult<IEnumerable<PromptDto>>> GetPromptsByLocale(string locale)
    {
        var result = await _promptService.GetByLocaleAsync(locale);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get filtered prompts with pagination
    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<PromptDto>>> GetFilteredPrompts([FromQuery] PromptListQueryDto query)
    {
        var result = await _promptService.GetFilteredPromptsAsync(query);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    #endregion
}
