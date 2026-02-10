using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.InterviewConfiguration;
using Recruiter.Application.InterviewConfiguration.Dto;
using Recruiter.Application.InterviewConfiguration.Interfaces;
using Recruiter.Application.Common.Dto;

namespace Recruiter.WebApi.Endpoints;

// InterviewConfiguration API Controller - handles all interview configuration operations (CRUD + Queries)
[ApiController]
[Route("api/[controller]")]
public class InterviewConfigurationController(IInterviewConfigurationService configurationService) : ControllerBase
{
    private readonly IInterviewConfigurationService _configurationService = configurationService;

    #region CRUD Operations

    // Get interview configuration by name and version
    [HttpGet("{name}/{version}")]
    public async Task<ActionResult<InterviewConfigurationDto>> GetConfigurationByNameAndVersion(string name, int version)
    {
        var configuration = await _configurationService.GetByIdAsync(name, version);
        if (configuration == null)
            return NotFound();
        return Ok(configuration);
    }

    // Get all interview configurations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterviewConfigurationDto>>> GetAllConfigurations()
    {
        var configurations = await _configurationService.GetAllAsync();
        return Ok(configurations);
    }

    // Create new interview configuration
    [HttpPost]
    public async Task<ActionResult<InterviewConfigurationDto>> CreateConfiguration([FromBody] InterviewConfigurationDto configurationDto)
    {
        if (configurationDto == null)
            return BadRequest("Configuration data is required");

        var createdConfiguration = await _configurationService.CreateAsync(configurationDto);
        return CreatedAtAction(nameof(GetConfigurationByNameAndVersion), 
            new { name = createdConfiguration.Name, version = createdConfiguration.Version }, 
            createdConfiguration);
    }

    // Update interview configuration
    [HttpPut("{name}/{version}")]
    public async Task<ActionResult<InterviewConfigurationDto>> UpdateConfiguration(string name, int version, [FromBody] InterviewConfigurationDto configurationDto)
    {
        if (configurationDto == null || name != configurationDto.Name || version != configurationDto.Version)
            return BadRequest("Invalid configuration data or name/version mismatch");

        var updatedConfiguration = await _configurationService.UpdateAsync(configurationDto);
        return Ok(updatedConfiguration);
    }

    // Duplicate interview configuration into a fresh v1 with a new name
    [HttpPost("{name}/{version}/duplicate")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<InterviewConfigurationDto>> DuplicateConfiguration(string name, int version, [FromBody] DuplicateInterviewConfigurationRequestDto request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var duplicated = await _configurationService.DuplicateAsync(name, version, request);
        if (duplicated == null)
            return NotFound();

        return CreatedAtAction(nameof(GetConfigurationByNameAndVersion),
            new { name = duplicated.Name, version = duplicated.Version },
            duplicated);
    }

    // Delete interview configuration
    [HttpDelete("{name}/{version}")]
    public async Task<ActionResult> DeleteConfiguration(string name, int version)
    {
        var exists = await _configurationService.ExistsAsync(name, version);
        if (!exists)
            return NotFound();

        await _configurationService.DeleteAsync(name, version);
        return NoContent();
    }

    // Check if configuration exists
    [HttpHead("{name}/{version}")]
    public async Task<ActionResult> ConfigurationExists(string name, int version)
    {
        var exists = await _configurationService.ExistsAsync(name, version);
        return exists ? Ok() : NotFound();
    }

    #endregion

    #region Query Operations

    // Get latest configuration by name
    [HttpGet("{name}/latest")]
    public async Task<ActionResult<InterviewConfigurationDto>> GetLatestConfigurationByName(string name)
    {
        var result = await _configurationService.GetLatestByNameAsync(name);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get configurations by modality
    [HttpGet("by-modality/{modality}")]
    public async Task<ActionResult<IEnumerable<InterviewConfigurationDto>>> GetConfigurationsByModality(string modality)
    {
        var result = await _configurationService.GetByModalityAsync(modality);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get active configurations
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<InterviewConfigurationDto>>> GetActiveConfigurations()
    {
        var result = await _configurationService.GetActiveConfigurationsAsync();
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get filtered configurations with pagination
    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<InterviewConfigurationDto>>> GetFilteredConfigurations([FromQuery] InterviewConfigurationListQueryDto query)
    {
        var result = await _configurationService.GetFilteredConfigurationsAsync(query);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    #endregion

    #region New Methods with Resolved Prompts

    // Get interview configuration with resolved prompts by name and version
    [HttpGet("{name}/{version}/with-prompts")]
    public async Task<ActionResult<InterviewConfigurationWithPromptsDto>> GetConfigurationWithPromptsByNameAndVersion(string name, int version)
    {
        var configuration = await _configurationService.GetWithResolvedPromptsAsync(name, version);
        if (configuration == null)
            return NotFound();
        return Ok(configuration);
    }

    // Get latest interview configuration with resolved prompts by name
    [HttpGet("{name}/latest/with-prompts")]
    public async Task<ActionResult<InterviewConfigurationWithPromptsDto>> GetLatestConfigurationWithPromptsByName(string name)
    {
        var configuration = await _configurationService.GetLatestWithResolvedPromptsAsync(name);
        if (configuration == null)
            return NotFound();
        return Ok(configuration);
    }

    // Get available versions for a prompt
    [HttpGet("prompt-versions/{promptName}")]
    public async Task<ActionResult<List<PromptVersionDto>>> GetPromptVersions(string promptName)
    {
        var versions = await _configurationService.GetPromptVersionsAsync(promptName);
        return Ok(versions);
    }

    #endregion
}
