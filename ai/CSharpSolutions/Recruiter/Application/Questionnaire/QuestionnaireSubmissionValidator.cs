using Ardalis.Result;
using Recruiter.Application.Questionnaire.Dto;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Questionnaire;

/// <summary>
/// Validates questionnaire submission requests.
/// </summary>
internal static class QuestionnaireSubmissionValidator
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

    public static List<ValidationError> ValidateRequest(
        CandidateQuestionnaireSubmitRequestDto request,
        Dictionary<(string Name, int Version), QuestionnaireQuestion> questionByKey)
    {
        var errors = new List<ValidationError>();
        var incomingByQuestionKey = (request.Answers ?? [])
            .GroupBy(a => (a.QuestionName, a.QuestionVersion))
            .ToDictionary(g => g.Key, g => g.Last());

        // Validate required questions
        foreach (var question in questionByKey.Values.Where(q => q.IsRequired).OrderBy(x => x.Order))
        {
            var questionKey = (question.Name, question.Version);
            if (!incomingByQuestionKey.TryGetValue(questionKey, out var incoming))
            {
                errors.Add(new ValidationError { ErrorMessage = $"Question '{question.QuestionText}' is required." });
                continue;
            }

            var hasAnswer = OptionBasedTypes.Contains(question.QuestionType)
                ? (incoming.SelectedOptions?.Count ?? 0) > 0
                : !string.IsNullOrWhiteSpace(incoming.AnswerText);

            if (!hasAnswer)
                errors.Add(new ValidationError { ErrorMessage = $"Question '{question.QuestionText}' is required." });
        }

        // Validate question and option references
        foreach (var answer in request.Answers ?? [])
        {
            var questionKey = (answer.QuestionName, answer.QuestionVersion);
            if (!questionByKey.ContainsKey(questionKey))
            {
                errors.Add(new ValidationError { ErrorMessage = "One or more answers reference an invalid question." });
                break;
            }

            if (!questionByKey.TryGetValue(questionKey, out var question))
                continue;

            var selected = (answer.SelectedOptions ?? []).ToList();
            var hasSelected = selected.Count > 0;

            // Prevent duplicate selections (would violate UX_QCSAnsOpt_Ans_OptName_OptVer and is logically invalid)
            var distinctSelected = selected.Select(o => (o.OptionName, o.OptionVersion)).Distinct().Count();
            if (distinctSelected != selected.Count)
            {
                errors.Add(new ValidationError { ErrorMessage = "Duplicate selected options are not allowed." });
                return errors;
            }

            // For non-option-based questions, selected options must be empty
            if (!OptionBasedTypes.Contains(question.QuestionType))
            {
                if (hasSelected)
                {
                    errors.Add(new ValidationError { ErrorMessage = "Selected options are not allowed for this question type." });
                    return errors;
                }
                continue;
            }

            // Enforce selection counts for single-choice types
            var isSingleChoice = question.QuestionType is QuestionnaireQuestionTypeEnum.SingleChoice
                or QuestionnaireQuestionTypeEnum.Radio
                or QuestionnaireQuestionTypeEnum.Dropdown
                or QuestionnaireQuestionTypeEnum.Likert;
            if (isSingleChoice && selected.Count > 1)
            {
                errors.Add(new ValidationError { ErrorMessage = "Only one option can be selected for this question." });
                return errors;
            }

            var optionKeys = (question.Options ?? []).Where(o => !o.IsDeleted)
                .Select(o => (o.Name, o.Version)).ToHashSet();

            if ((answer.SelectedOptions ?? []).Any(optRef => !optionKeys.Contains((optRef.OptionName, optRef.OptionVersion))))
            {
                errors.Add(new ValidationError { ErrorMessage = "One or more answers reference an invalid option." });
                return errors;
            }
        }

        return errors;
    }

    public static Result<T> ValidateSubmissionStatus<T>(
        QuestionnaireCandidateSubmission? submission,
        Recruiter.Domain.Models.QuestionnaireTemplate template,
        DateTimeOffset now)
    {
        if (submission != null && IsFinalStatus(submission.Status))
            return Result<T>.Invalid(
                new ValidationError { ErrorMessage = "This assessment has already been submitted and cannot be modified." });

        if (template.TimeLimitSeconds.HasValue && submission?.StartedAt.HasValue == true)
        {
            var elapsed = (now - submission.StartedAt.Value).TotalSeconds;
            if (elapsed > template.TimeLimitSeconds.Value)
                return Result<T>.Invalid(
                    new ValidationError { ErrorMessage = "Time limit for this assessment has expired." });
        }

        return Result<T>.Success(default!);
    }

    private static bool IsFinalStatus(QuestionnaireSubmissionStatusEnum status) =>
        status is QuestionnaireSubmissionStatusEnum.Submitted
            or QuestionnaireSubmissionStatusEnum.AutoScored
            or QuestionnaireSubmissionStatusEnum.Reviewed;
}
