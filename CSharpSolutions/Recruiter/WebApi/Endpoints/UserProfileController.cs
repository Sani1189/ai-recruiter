using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.UserProfile;
using Recruiter.Application.UserProfile.Dto;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.Candidate.Dto;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.WebApi.Attributes;
using Recruiter.Application.Common.Options;

namespace Recruiter.WebApi.Endpoints;

// UserProfile API Controller - handles all user profile operations (CRUD + Queries + Registration)
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrCandidate")]
public class UserProfileController(
    IUserProfileService userProfileService,
    IUserRegistrationService userRegistrationService,
    ICurrentUserService currentUserService,
    ICandidateService candidateService,
    IFileStorageService fileStorageService,
    AzureStorageOptions storageOptions) : ControllerBase
{
    private readonly IUserProfileService _userProfileService = userProfileService;
    private readonly IUserRegistrationService _userRegistrationService = userRegistrationService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICandidateService _candidateService = candidateService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly AzureStorageOptions _storageOptions = storageOptions;

    #region Admin Operations

    [HttpGet("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfileById(Guid id)
    {
        var userProfile = await _userProfileService.GetByIdAsync(id);
        if (userProfile == null)
            return NotFound();
        return Ok(userProfile);
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAllUserProfiles()
    {
        var userProfiles = await _userProfileService.GetAllAsync();
        return Ok(userProfiles);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> DeleteUserProfile(Guid id)
    {
        var exists = await _userProfileService.ExistsAsync(id);
        if (!exists)
            return NotFound();

        await _userProfileService.DeleteAsync(id);
        return NoContent();
    }

    #endregion


    #region User Operations

    [HttpGet("me")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        var userProfile = await _userRegistrationService.RegisterUserAsync(User);
        return Ok(userProfile);
    }

    [HttpPut("me")]
    [Authorize(Policy = "Authenticated")]
    public async Task<ActionResult<UserProfileDto>> UpdateCurrentUserProfile([FromBody] UserProfileDto userProfileDto)
    {
        if (userProfileDto == null)
            return BadRequest("User profile data is required");

        var userInfo = _currentUserService.GetCurrentUserInfo();
        if (string.IsNullOrEmpty(userInfo.Email))
            return Unauthorized("Unable to identify current user");

        var existingUserResult = await _userProfileService.GetByEmailAsync(userInfo.Email);
        if (!existingUserResult.IsSuccess || existingUserResult.Value == null)
            return NotFound("User profile not found");

        userProfileDto.Id = existingUserResult.Value.Id;
        var updatedUserProfile = await _userProfileService.UpdateCurrentUserProfileAsync(userProfileDto);
        return Ok(updatedUserProfile);
    }

    [HttpGet("me/candidate")]
    [Authorize(Policy = "Authenticated")]
    public async Task<ActionResult<CandidateDto>> GetCurrentUserCandidate()
    {
        var userInfo = _currentUserService.GetCurrentUserInfo();
        var userProfile = await _userRegistrationService.RegisterUserAsync(User);
        
        bool isAdmin = userInfo.Roles?.Any(role => 
            role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
            role.Equals("RecruitmentAdmin", StringComparison.OrdinalIgnoreCase)) == true;
        
        if (isAdmin)
        {
            return NotFound("Admin users don't have candidate records as they don't apply for jobs");
        }
        
        // Get candidate by user ID
        var candidate = await _candidateService.GetByUserIdWithUserProfileAsync(userProfile.Id!.Value);
        if (candidate == null)
        {
            return NotFound("Candidate record not found");
        }
        
        return Ok(candidate);
    }

    /// <summary>
    /// Get complete user profile details with all related data (for candidates)
    /// This endpoint returns all profile data in a single call to minimize API requests
    /// </summary>
    [HttpGet("me/details")]
    [Authorize(Policy = "Candidate")]
    public async Task<ActionResult<UserProfileDetailsDto>> GetCurrentUserProfileDetails()
    {
        // var userInfo = _currentUserService.GetCurrentUserInfo();
        var userProfile = await _userRegistrationService.RegisterUserAsync(User);
        
        Ardalis.Result.Result<UserProfileDetailsDto> result = await _userProfileService.GetUserProfileDetailsAsync(userProfile.Id!.Value);
        
        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
                return NotFound(result.Errors);
            if (result.Status == Ardalis.Result.ResultStatus.Invalid)
                return BadRequest(result.ValidationErrors);
            return StatusCode(500, result.Errors);
        }
        
        return Ok(result.Value);
    }

    [HttpPost("register")]
    [Authorize(Policy = "Authenticated")]
    public async Task<ActionResult<UserProfileDto>> RegisterUser()
    {
        var userProfile = await _userRegistrationService.RegisterUserAsync(User);
        return Ok(userProfile);
    }

    [HttpGet("info")]
    [Authorize(Policy = "Authenticated")]
    public ActionResult<UserInfoDto> GetUserInfo()
    {
        var userInfo = _currentUserService.GetCurrentUserInfo();
        return Ok(userInfo);
    }

    /// <summary>
    /// For Admin: Get resume file by candidate ID - returns a secure download URL
    /// </summary>
    [HttpGet("candidate/{id}/resume")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetResume(string id)
    {
        if (!Guid.TryParse(id, out var candidateId))
            return BadRequest("Invalid candidate ID");

        var result = await _candidateService.GetCandidateDetailsById(candidateId);
        var userProfile = result.Value?.UserProfile;

        if (!result.IsSuccess || userProfile == null || string.IsNullOrEmpty(userProfile.ResumeUrl))
            return NotFound("Resume not found");

        try
        {
            // Generate secure download URL (short-lived, read-once style)
            var downloadUrl = _fileStorageService.GenerateSecureDownloadUrl(
                _storageOptions.ContainerName, 
                userProfile.ResumeUrl, 
                expirationMinutes: 5);
            
            return Ok(new { downloadUrl, expiresInMinutes = 5 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating download URL", error = ex.Message });
        }
    }

    /// <summary>
    /// For Candidate: Get resume file by candidate ID - returns a secure download URL
    /// </summary>

    [HttpGet("me/resume")]
    [Authorize(Policy = "Candidate")]
    public async Task<IActionResult> GetMyResume()
    {
        var userProfile = await _currentUserService.GetUserAsync();
        if (userProfile == null || string.IsNullOrEmpty(userProfile.ResumeUrl))
            return NotFound("Resume not found");

        try
        {
            // Generate secure download URL (short-lived, read-once style)
            var downloadUrl = _fileStorageService.GenerateSecureDownloadUrl(
                _storageOptions.ContainerName, 
                userProfile.ResumeUrl, 
                expirationMinutes: 5);
            
            return Ok(new { downloadUrl, expiresInMinutes = 5 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating download URL", error = ex.Message });
        }
    }
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetResumeStream(string id)
    {
        if (!Guid.TryParse(id, out var candidateId))
            return BadRequest("Invalid candidate ID");

        var result = await _candidateService.GetCandidateDetailsById(candidateId);
        var userProfile = result.Value?.UserProfile;

        if (!result.IsSuccess || userProfile == null || string.IsNullOrEmpty(userProfile.ResumeUrl))
            return NotFound("Resume not found");

        try
        {
            var fileName = Path.GetFileName(userProfile.ResumeUrl) ?? "resume.pdf";
            await using var stream = await _fileStorageService.DownloadAsync(_storageOptions.ContainerName, userProfile.ResumeUrl);
            
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            return fileBytes.Length == 0 
                ? StatusCode(500, new { message = "Downloaded file is empty" })
                : File(fileBytes, GetContentType(fileName), fileName);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = "Resume file not found", error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = "Access denied", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error downloading resume", error = ex.Message });
        }
    }

    private static string GetContentType(string fileName)
    {
        return Path.GetExtension(fileName).ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    #endregion
}
