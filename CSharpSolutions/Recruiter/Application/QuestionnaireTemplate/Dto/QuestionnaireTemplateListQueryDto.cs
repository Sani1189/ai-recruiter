namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class QuestionnaireTemplateListQueryDto
{
    public string? SearchTerm { get; set; } // searches Name/Title
    public string? TemplateType { get; set; } // "Quiz" | "Personality" | "Form"
    public string? Status { get; set; } // "Draft" | "Published" | "Archived"

    /// <summary>
    /// Include soft-deleted templates (IsDeleted=true) in list results.
    /// Default: false (hide deleted).
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// When true, returns only soft-deleted templates (IsDeleted=true).
    /// </summary>
    public bool OnlyDeleted { get; set; } = false;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "UpdatedAt";
    public bool SortDescending { get; set; } = true;
}


