using FluentValidation;
using Recruiter.Application.InterviewConfiguration.Dto;

namespace Recruiter.Application.InterviewConfiguration.Validation;

public class InterviewConfigurationDtoValidator : AbstractValidator<InterviewConfigurationDto>
{
    public InterviewConfigurationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(x => x.Version)
            .GreaterThan(0)
            .WithMessage("Version must be greater than 0.");

        RuleFor(x => x.Modality)
            .NotEmpty()
            .WithMessage("Modality is required.");

        RuleFor(x => x.InstructionPromptName)
            .NotEmpty()
            .WithMessage("Instruction Prompt Name is required.");

        RuleFor(x => x.PersonalityPromptName)
            .NotEmpty()
            .WithMessage("Personality Prompt Name is required.");

        RuleFor(x => x.QuestionsPromptName)
            .NotEmpty()
            .WithMessage("Questions Prompt Name is required.");

        RuleFor(x => x.InstructionPromptVersion)
            .GreaterThan(0)
            .When(x => x.InstructionPromptVersion.HasValue)
            .WithMessage("Instruction Prompt Version must be greater than 0 when provided.");

        RuleFor(x => x.PersonalityPromptVersion)
            .GreaterThan(0)
            .When(x => x.PersonalityPromptVersion.HasValue)
            .WithMessage("Personality Prompt Version must be greater than 0 when provided.");

        RuleFor(x => x.QuestionsPromptVersion)
            .GreaterThan(0)
            .When(x => x.QuestionsPromptVersion.HasValue)
            .WithMessage("Questions Prompt Version must be greater than 0 when provided.");

        RuleFor(x => x.Duration)
            .GreaterThan(0)
            .When(x => x.Duration.HasValue)
            .WithMessage("Duration must be greater than 0 when provided.");
    }
}
