using FluentValidation;
using Recruiter.Application.QuestionnaireTemplate.Dto;

namespace Recruiter.Application.QuestionnaireTemplate.Validation;

public class QuestionnaireTemplateDtoValidator : AbstractValidator<QuestionnaireTemplateDto>
{
    private static readonly HashSet<string> AllowedTemplateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Quiz", "Personality", "Form"
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft", "Published", "Archived"
    };

    private static readonly HashSet<string> AllowedQuestionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Text", "Textarea", "Radio", "Checkbox", "Dropdown",
        "SingleChoice", "MultiChoice",
        "Likert"
    };

    public QuestionnaireTemplateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required")
            .MaximumLength(255).WithMessage("Template name cannot exceed 255 characters");

        RuleFor(x => x.Version)
            .GreaterThan(0).WithMessage("Version must be greater than zero");

        RuleFor(x => x.TemplateType)
            .NotEmpty()
            .Must(t => AllowedTemplateTypes.Contains(t))
            .WithMessage("TemplateType must be Quiz, Personality, or Form");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage("Status must be Draft, Published, or Archived");

        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.TimeLimitSeconds)
            .GreaterThan(0).WithMessage("TimeLimitSeconds must be greater than zero")
            .When(x => x.TimeLimitSeconds.HasValue);

        RuleFor(x => x.Sections)
            .NotNull().WithMessage("Sections are required");

        RuleForEach(x => x.Sections).ChildRules(section =>
        {
            section.RuleFor(s => s.Id).NotEmpty();
            section.RuleFor(s => s.Order).GreaterThan(0);
            section.RuleFor(s => s.Title).NotEmpty().MaximumLength(255);

            section.RuleForEach(s => s.Questions).ChildRules(question =>
            {
                // Name is optional - will be auto-generated from promptText if empty
                question.RuleFor(q => q.Order).GreaterThan(0);
                question.RuleFor(q => q.QuestionType)
                    .NotEmpty()
                    .Must(t => AllowedQuestionTypes.Contains(t))
                    .WithMessage("QuestionType is invalid");

                question.RuleFor(q => q.PromptText).NotEmpty().WithMessage("Question is required");

                // Likert requires Ws + TraitKey
                question.RuleFor(q => q.Ws)
                    .NotNull().WithMessage("Ws is required for Likert questions")
                    .When(q => string.Equals(q.QuestionType, "Likert", StringComparison.OrdinalIgnoreCase));
                question.RuleFor(q => q.TraitKey)
                    .NotEmpty().WithMessage("TraitKey is required for Likert questions")
                    .When(q => string.Equals(q.QuestionType, "Likert", StringComparison.OrdinalIgnoreCase));

                question.RuleForEach(q => q.Options).ChildRules(option =>
                {
                    // Name is optional - will be auto-generated from label if empty
                    option.RuleFor(o => o.Order).GreaterThan(0);
                    option.RuleFor(o => o.Label).NotEmpty().WithMessage("Option label is required");
                });
            });
        });
    }
}


