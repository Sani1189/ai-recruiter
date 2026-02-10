using FluentValidation;
using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Validation;

// FluentValidation validator for JobPostStepAssignmentDto
public class JobPostStepAssignmentDtoValidator : AbstractValidator<JobPostStepAssignmentDto>
{
    public JobPostStepAssignmentDtoValidator()
    {
        RuleFor(x => x.JobPostName)
            .NotEmpty().WithMessage("Job post name is required")
            .MaximumLength(255).WithMessage("Job post name cannot exceed 255 characters");

        RuleFor(x => x.JobPostVersion)
            .GreaterThan(0).WithMessage("Job post version must be greater than zero");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters")
            .Must(status => new[] { "pending", "in_progress", "completed", "skipped", "failed" }.Contains(status))
            .WithMessage("Status must be: pending, in_progress, completed, skipped, or failed");

        RuleFor(x => x.StepDetails)
            .NotNull().WithMessage("Step details are required");
    }
}
