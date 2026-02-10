using FluentValidation;
using Recruiter.Application.Prompt.Dto;

namespace Recruiter.Application.Prompt.Validation;

public class PromptDtoValidator : AbstractValidator<PromptDto>
{
    public PromptDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(x => x.Version)
            .GreaterThan(0)
            .WithMessage("Version must be greater than 0.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required.");

        RuleFor(x => x.Locale)
            .MaximumLength(10)
            .WithMessage("Locale cannot exceed 10 characters.");
    }
}
