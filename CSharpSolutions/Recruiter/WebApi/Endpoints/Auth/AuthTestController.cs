using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Recruiter.WebApi.Endpoints.Auth;

[ApiController]
[Route("api/auth-test")]
public class AuthTestController : ControllerBase
{
    [HttpGet("candidate")]
    [Authorize(Policy = "Candidate")]
    public IActionResult CandidateOnly()
    {
        return Ok(new
        {
            message = "Candidate token accepted",
            name = User.Identity?.Name,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    [HttpGet("admin")]
    [Authorize(Policy = "Admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            message = "Admin token accepted",
            name = User.Identity?.Name,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult AnyAuthenticated()
    {
        return Ok(new
        {
            message = "Authenticated token accepted",
            name = User.Identity?.Name,
            authType = User.Identity?.AuthenticationType,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public()
    {
        return Ok(new { message = "Public endpoint (no auth)" });
    }
}


