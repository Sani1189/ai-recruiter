using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate;

public static class QuestionnaireTemplateChangeDetector
{
    public static bool HasQuestionChanged(QuestionnaireQuestion existing, QuestionnaireQuestionDto incoming)
    {
        return existing.QuestionText != (incoming.PromptText?.Trim() ?? string.Empty) ||
               existing.Order != incoming.Order ||
               existing.IsRequired != incoming.IsRequired ||
               existing.TraitKey != incoming.TraitKey ||
               existing.Ws != incoming.Ws ||
               existing.MediaUrl != incoming.MediaUrl ||
               existing.MediaFileId != incoming.MediaFileId ||
               (Enum.TryParse<Domain.Enums.QuestionnaireQuestionTypeEnum>(incoming.QuestionType, true, out var questionType) && existing.QuestionType != questionType);
    }

    public static bool HasOptionChanged(QuestionnaireQuestionOption existing, QuestionnaireOptionDto incoming)
    {
        return existing.Label != (incoming.Label?.Trim() ?? string.Empty) ||
               existing.Order != incoming.Order ||
               existing.MediaUrl != incoming.MediaUrl ||
               existing.MediaFileId != incoming.MediaFileId ||
               existing.IsCorrect != incoming.IsCorrect ||
               existing.Score != incoming.Score ||
               existing.Weight != incoming.Weight ||
               existing.Wa != incoming.Wa;
    }
}
