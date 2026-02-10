using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.InterviewConfiguration.Dto;
using Recruiter.Application.InterviewConfiguration.Interfaces;

namespace Recruiter.WebApi.Endpoints.Candidate;

// Candidate Interview Configuration API Controller - handles interview configuration access for candidates
[ApiController]
[Route("api/candidate/interview-configuration")]
public class InterviewConfigurationController(IInterviewConfigurationService configurationService) : ControllerBase
{
    private readonly IInterviewConfigurationService _configurationService = configurationService;

    [HttpGet("check-auth")]
    [Authorize(Policy = "Candidate")]
    public IActionResult CheckAuth()
    {
        return Ok(new { message = "Authentication successful" });
    }
    // Get interview configuration with resolved prompts by name and version - PUBLIC for candidates
    [AllowAnonymous]
    [HttpGet("{name}/{version}/with-prompts")]
    public async Task<ActionResult<InterviewConfigurationWithPromptsDto>> GetConfigurationWithPromptsByNameAndVersion(string name, int version)
    {
        var configuration = await _configurationService.GetWithResolvedPromptsAsync(name, version);
        if (configuration == null)
            return NotFound();
        return Ok(configuration);
    }
    

    // Get latest interview configuration with resolved prompts by name - PUBLIC for candidates
    [AllowAnonymous]
    [HttpGet("{name}/latest/with-prompts")]
    public async Task<ActionResult<InterviewConfigurationWithPromptsDto>> GetLatestConfigurationWithPromptsByName(string name)
    {
        var configuration = await _configurationService.GetLatestWithResolvedPromptsAsync(name);
        if (configuration == null)
            return NotFound();
        return Ok(configuration);
    }
}
