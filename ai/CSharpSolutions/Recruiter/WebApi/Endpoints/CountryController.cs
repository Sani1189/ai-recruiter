using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.Country.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrCandidate")]
public class CountryController(ICountryListService countryListService) : ControllerBase
{
    /// <summary>
    /// Get active countries for dropdowns (origin country, job post country exposure).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Recruiter.Application.Country.Dto.CountryListItemDto>>> GetCountries(CancellationToken cancellationToken = default)
    {
        var list = await countryListService.GetActiveForDropdownAsync(cancellationToken);
        return Ok(list);
    }
}
