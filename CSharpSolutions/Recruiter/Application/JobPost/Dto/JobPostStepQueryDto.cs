using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobPost.Dto;

public class JobPostStepQueryDto : VersionedBaseQueryDto
{
    public string? SearchTerm { get; set; } // Searches in Name, StepType, InterviewConfigurationName
    
    [RegularExpression("^(Screening|Technical|Behavioral|Assignment|Interview)$")]
    public string? StepType { get; set; }
    
    // Pagination
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
    
    // Sorting (case-insensitive, handled in specification)
    public string? SortBy { get; set; } = "CreatedAt";
    
    public bool SortDescending { get; set; } = true;
}
