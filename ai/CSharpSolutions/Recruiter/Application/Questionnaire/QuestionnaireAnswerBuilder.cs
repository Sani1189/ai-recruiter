using Recruiter.Application.Questionnaire.Dto;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Questionnaire;

/// <summary>
/// Builds questionnaire submission answers with scoring calculations.
/// </summary>
internal static class QuestionnaireAnswerBuilder
{
    private static readonly HashSet<QuestionnaireQuestionTypeEnum> OptionBasedTypes = new()
    {
        QuestionnaireQuestionTypeEnum.SingleChoice,
        QuestionnaireQuestionTypeEnum.MultiChoice,
        QuestionnaireQuestionTypeEnum.Likert,
        QuestionnaireQuestionTypeEnum.Radio,
        QuestionnaireQuestionTypeEnum.Checkbox,
        QuestionnaireQuestionTypeEnum.Dropdown
    };

    public static AnswerBuildResult BuildAnswers(
        List<CandidateQuestionnaireAnswerDto> incomingAnswers,
        Dictionary<(string Name, int Version), QuestionnaireQuestion> questionByKey,
        QuestionnaireTemplateTypeEnum templateType,
        DateTimeOffset now)
    {
        var totalScore = 0m;
        var maxScore = 0m;
        var hasScoredQuestions = false;
        var answers = new List<QuestionnaireCandidateSubmissionAnswer>();
        var isQuiz = templateType == QuestionnaireTemplateTypeEnum.Quiz;
        var isPersonality = templateType == QuestionnaireTemplateTypeEnum.Personality;

        foreach (var incoming in incomingAnswers)
        {
            var questionKey = (incoming.QuestionName, incoming.QuestionVersion);
            if (!questionByKey.TryGetValue(questionKey, out var question))
                continue;

            var answer = CreateAnswer(question, incoming, now);

            if (OptionBasedTypes.Contains(question.QuestionType))
            {
                ProcessOptions(answer, incoming, question, isQuiz, isPersonality, ref totalScore, ref maxScore, ref hasScoredQuestions);
            }

            answers.Add(answer);
        }

        return new AnswerBuildResult(totalScore, maxScore, hasScoredQuestions, answers);
    }

    private static QuestionnaireCandidateSubmissionAnswer CreateAnswer(
        QuestionnaireQuestion question,
        CandidateQuestionnaireAnswerDto incoming,
        DateTimeOffset now)
    {
        return new QuestionnaireCandidateSubmissionAnswer
        {
            Id = Guid.NewGuid(),
            QuestionnaireQuestionName = question.Name,
            QuestionnaireQuestionVersion = question.Version,
            QuestionType = question.QuestionType,
            QuestionOrder = question.Order,
            AnswerText = NormalizeText(incoming.AnswerText),
            AnsweredAt = now
        };
    }

    private static void ProcessOptions(
        QuestionnaireCandidateSubmissionAnswer answer,
        CandidateQuestionnaireAnswerDto incoming,
        QuestionnaireQuestion question,
        bool isQuiz,
        bool isPersonality,
        ref decimal totalScore,
        ref decimal maxScore,
        ref bool hasScoredQuestions)
    {
        var selected = (incoming.SelectedOptions ?? []).ToList();
        var optionByKey = (question.Options ?? []).Where(o => !o.IsDeleted)
            .ToDictionary(o => (o.Name, o.Version), o => o);
        var quizCorrectnessConfigured = isQuiz && optionByKey.Values.Any(o => o.IsCorrect.HasValue);
        var questionWeight = question.Ws ?? 1.0m;
        var isLikert = question.QuestionType == QuestionnaireQuestionTypeEnum.Likert;
        var isSingleChoice = question.QuestionType is QuestionnaireQuestionTypeEnum.SingleChoice
            or QuestionnaireQuestionTypeEnum.Radio
            or QuestionnaireQuestionTypeEnum.Dropdown
            or QuestionnaireQuestionTypeEnum.Likert;
        var isMultiChoice = question.QuestionType is QuestionnaireQuestionTypeEnum.MultiChoice
            or QuestionnaireQuestionTypeEnum.Checkbox;
        var isScoredQuestion = optionByKey.Values.Any(o => o.Score.HasValue);

        decimal? answerWaSum = null;
        if (isLikert || isPersonality)
        {
            answerWaSum = CalculateWeightedAverage(selected, optionByKey, questionWeight, isSingleChoice);
        }

        var answerScore = 0m;
        foreach (var optionRef in selected)
        {
            var optionKey = (optionRef.OptionName, optionRef.OptionVersion);
            if (!optionByKey.TryGetValue(optionKey, out var opt))
                continue;

            var derivedIsCorrect = quizCorrectnessConfigured
                ? opt.IsCorrect
                : (isQuiz ? (opt.Score.HasValue && opt.Score.Value > 0m) : opt.IsCorrect);

            answer.SelectedOptions.Add(new QuestionnaireCandidateSubmissionAnswerOption
            {
                Id = Guid.NewGuid(),
                QuestionnaireCandidateSubmissionAnswerId = answer.Id,
                QuestionnaireQuestionOptionName = opt.Name,
                QuestionnaireQuestionOptionVersion = opt.Version,
                IsCorrect = derivedIsCorrect,
                Score = opt.Score,
                Wa = opt.Wa
            });

            if (opt.Score.HasValue)
            {
                // Quiz scoring rule:
                // - If correctness is configured (IsCorrect has values), only correct selections score.
                // - If not configured (all null), fall back to score-based scoring (common in older data).
                if (!isQuiz || !quizCorrectnessConfigured || derivedIsCorrect == true)
                {
                    answerScore += opt.Score.Value;
                }
            }
        }

        if ((isLikert || isPersonality) && answerWaSum.HasValue)
            answer.WaSum = answerWaSum.Value;

        if (isScoredQuestion)
        {
            hasScoredQuestions = true;
            answer.ScoreAwarded = answerScore;
            totalScore += answerScore;
            maxScore += CalculateMaxScore(question, isQuiz);
        }
    }

    private static decimal? CalculateWeightedAverage(
        List<QuestionOptionReferenceDto> selected,
        Dictionary<(string Name, int Version), QuestionnaireQuestionOption> optionByKey,
        decimal questionWeight,
        bool isSingleChoice)
    {
        var weightedValues = selected
            .Select(optionRef =>
            {
                var optionKey = (optionRef.OptionName, optionRef.OptionVersion);
                return optionByKey.TryGetValue(optionKey, out var opt) && opt.Wa.HasValue
                    ? opt.Wa.Value * questionWeight
                    : (decimal?)null;
            })
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        if (weightedValues.Count == 0)
            return null;

        return isSingleChoice || weightedValues.Count == 1
            ? weightedValues[0]
            : weightedValues.Average();
    }

    private static decimal CalculateMaxScore(QuestionnaireQuestion question, bool isQuiz)
    {
        var validOptions = (question.Options ?? []).Where(o => !o.IsDeleted && o.Score.HasValue).ToList();
        if (validOptions.Count == 0)
            return 0m;

        var hasCorrectness = isQuiz && validOptions.Any(o => o.IsCorrect.HasValue);

        return question.QuestionType switch
        {
            QuestionnaireQuestionTypeEnum.SingleChoice or QuestionnaireQuestionTypeEnum.Radio or QuestionnaireQuestionTypeEnum.Dropdown
                => hasCorrectness
                    ? validOptions.Where(o => o.IsCorrect == true).Select(o => o.Score!.Value).DefaultIfEmpty(0m).Max()
                    : validOptions.Max(o => o.Score!.Value),
            QuestionnaireQuestionTypeEnum.MultiChoice or QuestionnaireQuestionTypeEnum.Checkbox
                => hasCorrectness
                    ? validOptions.Where(o => o.IsCorrect == true).Sum(o => o.Score!.Value)
                    : validOptions.Where(o => o.Score!.Value > 0m).Sum(o => o.Score!.Value),
            QuestionnaireQuestionTypeEnum.Likert
                => validOptions.Max(o => o.Score ?? 0m),
            _ => 0m
        };
    }

    private static string? NormalizeText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public record AnswerBuildResult(
        decimal TotalScore,
        decimal MaxScore,
        bool HasScoredQuestions,
        List<QuestionnaireCandidateSubmissionAnswer> Answers);
}
