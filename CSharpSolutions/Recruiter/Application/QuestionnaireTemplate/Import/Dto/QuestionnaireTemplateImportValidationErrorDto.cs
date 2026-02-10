namespace Recruiter.Application.QuestionnaireTemplate.Import.Dto;

public sealed class QuestionnaireTemplateImportValidationErrorDto
{
    public int RowNumber { get; init; }
    public string? Column { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "Error"; // Error | Warning
}

