using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.Interview.Dto;

public class CompleteInterviewDto
{
    [Range(1, 14400000, ErrorMessage = "Duration must be between 1 and 14400000 milliseconds (4 hours)")]
    public long? Duration { get; set; } // in milliseconds
    
    [MaxLength(500)]
    public string? TranscriptUrl { get; set; }
    
    [MaxLength(500)]
    public string? InterviewAudioUrl { get; set; }
    
    public List<string>? InterviewQuestions { get; set; }
    
    public string? Notes { get; set; }
}

