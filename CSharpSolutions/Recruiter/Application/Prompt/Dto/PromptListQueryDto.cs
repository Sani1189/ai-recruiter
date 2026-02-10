namespace Recruiter.Application.Prompt.Dto;

public class PromptListQueryDto
{
    public string? SearchTerm { get; set; } // Searches in Name, Category, Locale, Content
    public string? Category { get; set; }
    public string? Locale { get; set; }
    public string? Name { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
