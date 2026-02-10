namespace Recruiter.Application.InterviewConfiguration.Dto;

public class InterviewConfigurationListQueryDto
{
    public string? SearchTerm { get; set; } // Searches in Name, Modality, Tone, FocusArea
    public string? Modality { get; set; }
    public string? Tone { get; set; }
    public string? ProbingDepth { get; set; }
    public string? FocusArea { get; set; }
    public string? Language { get; set; }
    public bool? Active { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
