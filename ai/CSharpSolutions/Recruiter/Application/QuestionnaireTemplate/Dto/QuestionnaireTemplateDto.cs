namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public class QuestionnaireTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }

    public string TemplateType { get; set; } = "Form"; // "Quiz" | "Personality" | "Form"
    public string Status { get; set; } = "Draft";      // "Draft" | "Published" | "Archived"

    // Soft-delete flag (used for archive/hide behavior while preserving runtime compatibility).
    public bool IsDeleted { get; set; }

    // Convenience for frontend lists
    public bool IsPublished { get; set; }
    public int SectionsCount { get; set; }
    public int QuestionsCount { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? TimeLimitSeconds { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Update semantics: if true, create a new version (v+1) and keep current immutable.
    public bool? ShouldUpdateVersion { get; set; }

    public List<QuestionnaireSectionDto> Sections { get; set; } = new();
}


