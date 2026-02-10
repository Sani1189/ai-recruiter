using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.Dashboard.Dto;
using Recruiter.Application.Dashboard.Interfaces;
using Ardalis.Result;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Admin")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    private readonly IDashboardService _dashboardService = dashboardService;

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var result = await _dashboardService.GetDashboardStatsAsync();
        
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
            
        return Ok(result.Value);
    }
}

