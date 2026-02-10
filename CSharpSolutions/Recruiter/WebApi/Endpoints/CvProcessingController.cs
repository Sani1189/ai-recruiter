using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.CvProcessing.Dto;
using Recruiter.Application.CvProcessing.Interfaces;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.UserProfile.Interfaces;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Candidate")]
public class CvProcessingController(
    ICvProcessingService cvProcessingService,
    IUserProfileService userProfileService,
    ICurrentUserService currentUserService) : ControllerBase
{
    private readonly ICvProcessingService _cvProcessingService = cvProcessingService;
    private readonly IUserProfileService _userProfileService = userProfileService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<CvUploadResponseDto>> UploadCv(
        IFormFile file,
        [FromForm] string? promptCategory = "cv_extraction",
        [FromForm] string? promptName = "CVExtractionScoringInstructions",
        [FromForm] int promptVersion = 1)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");

        const long maxFileSize = 10_485_760;
        if (file.Length > maxFileSize)
            return BadRequest($"File too large. Maximum size: {maxFileSize / (1024 * 1024)}MB");

        var userInfo = _currentUserService.GetCurrentUserInfo();
        if (string.IsNullOrEmpty(userInfo.Email))
            return Unauthorized("Unable to identify current user");

        var existingUserResult = await _userProfileService.GetByEmailAsync(userInfo.Email);
        if (!existingUserResult.IsSuccess || existingUserResult.Value == null)
            return NotFound("User profile not found");

        var userProfileId = existingUserResult.Value.Id!.Value;

        try
        {
            var contentType = file.ContentType ?? GetContentType(file.FileName);
            var result = await _cvProcessingService.UploadCvAsync(
                file.OpenReadStream(),
                file.FileName,
                contentType,
                userProfileId,
                promptCategory ?? "cv_extraction",
                promptName ?? "CVExtractionScoringInstructions", 
                promptVersion);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Errors?.FirstOrDefault() 
                    ?? result.ValidationErrors?.FirstOrDefault()?.ErrorMessage 
                    ?? "CV upload failed";
                return StatusCode(500, new { message = errorMessage });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while uploading CV", error = ex.Message });
        }
    }

    private static string GetContentType(string fileName) => Path.GetExtension(fileName).ToLower() switch
    {
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".txt" => "text/plain",
        _ => "application/octet-stream"
    };
}

