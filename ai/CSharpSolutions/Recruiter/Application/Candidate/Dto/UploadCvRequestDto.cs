using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Candidate.Dto;

/// <summary>
/// Request DTO for uploading CV file to candidate
/// </summary>
public class UploadCvRequestDto
{
    [Required(ErrorMessage = "CV File ID is required")]
    public Guid CvFileId { get; set; }
}
