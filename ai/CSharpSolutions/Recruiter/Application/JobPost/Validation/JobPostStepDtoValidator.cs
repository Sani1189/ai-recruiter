using FluentValidation;
using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Validation;

// FluentValidation validator for JobPostStepDto
public class JobPostStepDtoValidator : AbstractValidator<JobPostStepDto>
{
    public JobPostStepDtoValidator()
    {
        // Back-compat: older payloads used "Assessment"/"Multiple Choice" for Questionnaire steps.
        var candidateStepTypes = new[] { "Interview", "Resume Upload", "Questionnaire", "Assessment", "Multiple Choice" };
        var recruiterStepTypes = new[] { "Ranking", "Documentation", "Generic" };

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Step name is required")
            .MaximumLength(255).WithMessage("Step name cannot exceed 255 characters");

        RuleFor(x => x.Version)
            .GreaterThan(0).WithMessage("Version must be greater than zero");

        RuleFor(x => x.Participant)
            .NotEmpty().WithMessage("Participant is required")
            .Must(p => p is "Candidate" or "Recruiter")
            .WithMessage("Participant must be: Candidate or Recruiter");

        RuleFor(x => x.StepType)
            .NotEmpty().WithMessage("Step type is required")
            .Must((dto, stepType) =>
            {
                if (dto.Participant == "Candidate")
                {
                    return candidateStepTypes.Contains(stepType);
                }

                if (dto.Participant == "Recruiter")
                {
                    return recruiterStepTypes.Contains(stepType);
                }

                return false;
            })
            .WithMessage("Step type is not valid for the selected participant");

        RuleFor(x => x.DisplayTitle)
            .MaximumLength(255).WithMessage("Display title cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.DisplayTitle));

        RuleFor(x => x.DisplayTitle)
            .Empty()
            .When(x => !x.ShowStepForCandidate)
            .WithMessage("Display title must be empty when ShowStepForCandidate is false");

        RuleFor(x => x.DisplayContent)
            .Empty()
            .When(x => !x.ShowStepForCandidate)
            .WithMessage("Display content must be empty when ShowStepForCandidate is false");

        RuleFor(x => x.ShowSpinner)
            .Equal(false)
            .When(x => !x.ShowStepForCandidate)
            .WithMessage("ShowSpinner must be false when ShowStepForCandidate is false");

        RuleFor(x => x.InterviewConfigurationName)
            .NotEmpty()
            .When(x => x.Participant == "Candidate" && x.StepType == "Interview")
            .WithMessage("Interview configuration name is required for Interview steps");

        RuleFor(x => x.InterviewConfigurationName)
            .Empty()
            .When(x => x.Participant == "Recruiter")
            .WithMessage("Recruiter steps cannot have an interview configuration");

        RuleFor(x => x.InterviewConfigurationName)
            .Empty()
            .When(x => x.Participant == "Candidate" && x.StepType != "Interview")
            .WithMessage("Only Interview steps can have an interview configuration");

        RuleFor(x => x.InterviewConfigurationVersion)
            .Empty()
            .When(x => x.Participant == "Recruiter")
            .WithMessage("Recruiter steps cannot have an interview configuration version");

        RuleFor(x => x.InterviewConfigurationVersion)
            .Empty()
            .When(x => x.Participant == "Candidate" && x.StepType != "Interview")
            .WithMessage("Only Interview steps can have an interview configuration version");

        // Candidate steps don't use spinner configuration
        RuleFor(x => x.ShowSpinner)
            .Equal(false)
            .When(x => x.Participant == "Candidate")
            .WithMessage("ShowSpinner is not supported for candidate steps");

        // Candidate steps are always visible to the candidate (flag is only meaningful for recruiter steps)
        RuleFor(x => x.ShowStepForCandidate)
            .Equal(true)
            .When(x => x.Participant == "Candidate")
            .WithMessage("Candidate steps are always visible to the candidate");

        RuleFor(x => x.InterviewConfigurationName)
            .MaximumLength(255).WithMessage("Interview configuration name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.InterviewConfigurationName));

        RuleFor(x => x.InterviewConfigurationVersion)
            .GreaterThan(0).WithMessage("Interview configuration version must be greater than zero")
            .When(x => x.InterviewConfigurationVersion.HasValue);

        RuleFor(x => x.QuestionnaireTemplateName)
            .NotEmpty()
            .When(x => x.Participant == "Candidate" && x.StepType is "Questionnaire" or "Assessment")
            .WithMessage("Questionnaire template name is required for Questionnaire steps");

        RuleFor(x => x.QuestionnaireTemplateName)
            .Empty()
            .When(x => x.Participant == "Recruiter")
            .WithMessage("Recruiter steps cannot have a questionnaire template");

        RuleFor(x => x.QuestionnaireTemplateName)
            .Empty()
            .When(x => x.Participant == "Candidate" && x.StepType is not ("Questionnaire" or "Assessment" or "Multiple Choice"))
            .WithMessage("Only Questionnaire steps can have a questionnaire template");

        RuleFor(x => x.QuestionnaireTemplateVersion)
            .Empty()
            .When(x => x.Participant == "Recruiter")
            .WithMessage("Recruiter steps cannot have a questionnaire template version");

        RuleFor(x => x.QuestionnaireTemplateVersion)
            .Empty()
            .When(x => x.Participant == "Candidate" && x.StepType is not ("Questionnaire" or "Assessment" or "Multiple Choice"))
            .WithMessage("Only Questionnaire steps can have a questionnaire template version");

        RuleFor(x => x.QuestionnaireTemplateName)
            .MaximumLength(255).WithMessage("Questionnaire template name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.QuestionnaireTemplateName));

        RuleFor(x => x.QuestionnaireTemplateVersion)
            .GreaterThan(0).WithMessage("Questionnaire template version must be greater than zero")
            .When(x => x.QuestionnaireTemplateVersion.HasValue);

        RuleFor(x => x.PromptName)
            .MaximumLength(255).WithMessage("Prompt name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.PromptName));

        RuleFor(x => x.PromptVersion)
            .GreaterThan(0).WithMessage("Prompt version must be greater than zero")
            .When(x => x.PromptVersion.HasValue);
    }
}
