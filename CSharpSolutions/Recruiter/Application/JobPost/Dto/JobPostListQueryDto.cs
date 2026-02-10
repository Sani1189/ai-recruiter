using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobPost.Dto;

public class JobPostListQueryDto : VersionedBaseQueryDto
{
    public string? SearchTerm { get; set; } // Searches in Name, JobTitle, JobDescription, JobType
    
    [RegularExpression("^(Entry|Mid|Senior|Lead|Executive)$")]
    public string? ExperienceLevel { get; set; }
    
    [MaxLength(200)]
    public string? JobTitle { get; set; }
    
    [RegularExpression("^(FullTime|PartTime|Contract|Internship)$")]
    public string? JobType { get; set; }
    
    public bool? PoliceReportRequired { get; set; }
    public DateTimeOffset? CreatedAfter { get; set; }
    public DateTimeOffset? CreatedBefore { get; set; }
    
    // Pagination
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
    
    // Sorting
    public string? SortBy { get; set; } = "CreatedAt";
    
    public bool SortDescending { get; set; } = true;
}