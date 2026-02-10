namespace Recruiter.Application.JobApplicationStepFiles.Dto;

public class GetUploadUrlRequestDto
{
    public string JobPostName { get; set; } = string.Empty;
    public int JobPostVersion { get; set; }
    public string StepName { get; set; } = string.Empty;
    public int? StepVersion { get; set; } // Null means use latest version
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

