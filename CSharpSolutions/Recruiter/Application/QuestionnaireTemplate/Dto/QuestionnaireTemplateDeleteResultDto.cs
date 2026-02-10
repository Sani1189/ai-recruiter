namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public sealed class QuestionnaireTemplateDeleteResultDto
{
    /// <summary>
    /// "Deleted" when hard-deleted from DB, "Archived" when soft-deleted (IsDeleted=true) to preserve references/history.
    /// </summary>
    public string Mode { get; init; } = "Deleted";
}

