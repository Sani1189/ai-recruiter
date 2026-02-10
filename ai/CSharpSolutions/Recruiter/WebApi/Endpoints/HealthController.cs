using Microsoft.AspNetCore.Mvc;
using Recruiter.WebApi.Attributes;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[PublicApi]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            data = new {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow.ToString("O"),
                Version = "1.0.0"
            },
            success = true,
            message = "Health check successful"
        });
    }
}
