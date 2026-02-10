using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Interview.Dto;

public class CreateInterviewDto
{
    [Required]
    public string InterviewConfigurationName { get; set; } = string.Empty;

    [Required]
    public int InterviewConfigurationVersion { get; set; }

    public string? TranscriptUrl { get; set; }

    public List<string>? InterviewQuestions { get; set; }

    public string? InterviewAudioUrl { get; set; }

    public int? Duration { get; set; }

    public string? Notes { get; set; }
}


