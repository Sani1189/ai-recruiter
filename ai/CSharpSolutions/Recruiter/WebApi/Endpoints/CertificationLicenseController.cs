using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.CertificationLicense.Dto;
using Recruiter.Application.CertificationLicense.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrCandidate")]
public class CertificationLicenseController(ICertificationLicenseService certificationLicenseService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly ICertificationLicenseService _certificationLicenseService = certificationLicenseService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    // Get all certifications/licenses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CertificationLicenseDto>>> GetAllCertificationsLicenses()
    {
        var certificationsLicenses = await _certificationLicenseService.GetAllAsync();
        return Ok(certificationsLicenses);
    }

    // Get certifications/licenses by user profile ID (for current user)
    [HttpGet("user-profile")]
    public async Task<ActionResult<IEnumerable<CertificationLicenseDto>>> GetCertificationsLicensesByUserProfileId()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null)
            return Unauthorized();
        var result = await _certificationLicenseService.GetByUserProfileIdAsync(userProfile.Id!.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get certifications/licenses by user profile ID (admin endpoint for viewing candidate data)
    [HttpGet("user-profile/{userProfileId}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<CertificationLicenseDto>>> GetCertificationsLicensesByUserProfileIdForAdmin(Guid userProfileId)
    {
        var result = await _certificationLicenseService.GetByUserProfileIdAsync(userProfileId);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    // Get certification/license by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<CertificationLicenseDto>> GetCertificationLicenseById(Guid id)
    {
        var certificationLicense = await _certificationLicenseService.GetByIdAsync(id);
        if (certificationLicense == null)
            return NotFound();
        return Ok(certificationLicense);
    }

    // Create certification/license
    [HttpPost]
    public async Task<ActionResult<CertificationLicenseDto>> CreateCertificationLicense([FromBody] CertificationLicenseDto certificationLicenseDto)
    {
        if (certificationLicenseDto == null)
            return BadRequest("Certification/License data is required");

        var createdCertificationLicense = await _certificationLicenseService.CreateAsync(certificationLicenseDto);
        return CreatedAtAction(nameof(GetCertificationLicenseById), new { id = createdCertificationLicense.Id }, createdCertificationLicense);
    }

    // Update certification/license
    [HttpPut("{id}")]
    public async Task<ActionResult<CertificationLicenseDto>> UpdateCertificationLicense(Guid id, [FromBody] CertificationLicenseDto certificationLicenseDto)
    {
        if (certificationLicenseDto == null)
            return BadRequest("Certification/License data is required");

        certificationLicenseDto.Id = id;

        var updatedCertificationLicense = await _certificationLicenseService.UpdateAsync(certificationLicenseDto);
        return Ok(updatedCertificationLicense);
    }

    // Delete certification/license
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCertificationLicense(Guid id)
    {
        var exists = await _certificationLicenseService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _certificationLicenseService.DeleteAsync(id);
        return NoContent();
    }
}
