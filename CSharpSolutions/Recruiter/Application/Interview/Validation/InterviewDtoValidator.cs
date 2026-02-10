using FluentValidation;
using Recruiter.Application.Interview.Dto;

namespace Recruiter.Application.Interview.Validation;

public class InterviewDtoValidator : AbstractValidator<InterviewDto>
{
    public InterviewDtoValidator()
    {
        RuleFor(x => x.JobApplicationStepId)
            .NotEmpty()
            .WithMessage("Job Application Step ID is required.");

        RuleFor(x => x.InterviewConfigurationName)
            .NotEmpty()
            .WithMessage("Interview Configuration Name is required.");

        RuleFor(x => x.InterviewConfigurationVersion)
            .GreaterThan(0)
            .WithMessage("Interview Configuration Version must be greater than 0.");

        RuleFor(x => x.InterviewAudioUrl)
            .MaximumLength(500)
            .WithMessage("Interview Audio URL cannot exceed 500 characters.");

        RuleFor(x => x.TranscriptUrl)
            .MaximumLength(500)
            .WithMessage("Transcript URL cannot exceed 500 characters.");

        RuleFor(x => x.Duration)
            .GreaterThan(0)
            .When(x => x.Duration.HasValue)
            .WithMessage("Duration must be greater than 0 when provided.");
    }
}
