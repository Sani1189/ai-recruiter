using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Questionnaire;

/// <summary>
/// Calculates personality trait scores from questionnaire answers.
/// </summary>
internal static class PersonalityScoreCalculator
{
    public static string? Calculate(
        List<QuestionnaireCandidateSubmissionAnswer> answers,
        Dictionary<(string Name, int Version), QuestionnaireQuestion> questionByKey,
        QuestionnaireTemplateTypeEnum templateType)
    {
        if (answers == null || answers.Count == 0)
            return null;

        var isPersonality = templateType == QuestionnaireTemplateTypeEnum.Personality;
        var hasTraitKeyQuestions = questionByKey.Values
            .Any(q => !string.IsNullOrWhiteSpace(q.TraitKey) && q.QuestionType == QuestionnaireQuestionTypeEnum.Likert);

        if (!isPersonality && !hasTraitKeyQuestions)
            return null;

        var traitGroups = answers
            .Where(a => HasTraitKey(a, questionByKey) && a.WaSum.HasValue)
            .GroupBy(a => questionByKey[(a.QuestionnaireQuestionName, a.QuestionnaireQuestionVersion)].TraitKey!)
            .ToList();

        if (traitGroups.Count == 0)
            return null;

        var traitScores = new Dictionary<string, decimal>();
        foreach (var group in traitGroups)
        {
            var traitAnswers = group.Where(a => a.WaSum.HasValue).ToList();
            if (traitAnswers.Count == 0)
                continue;

            var totalWaSum = traitAnswers.Sum(a => a.WaSum!.Value);
            var traitScore = totalWaSum / traitAnswers.Count;
            traitScores[group.Key] = Math.Round(traitScore, 2);
        }

        return traitScores.Count == 0
            ? null
            : System.Text.Json.JsonSerializer.Serialize(traitScores);
    }

    private static bool HasTraitKey(
        QuestionnaireCandidateSubmissionAnswer answer,
        Dictionary<(string Name, int Version), QuestionnaireQuestion> questionByKey)
    {
        var questionKey = (answer.QuestionnaireQuestionName, answer.QuestionnaireQuestionVersion);
        return questionByKey.TryGetValue(questionKey, out var question)
               && !string.IsNullOrWhiteSpace(question.TraitKey);
    }
}
