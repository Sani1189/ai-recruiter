using FluentValidation;
using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Validation;

// FluentValidation validator for JobPostDto
public class JobPostDtoValidator : AbstractValidator<JobPostDto>
{
    public JobPostDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Job post name is required from validator")
            .MaximumLength(255).WithMessage("Job post name cannot exceed 255 characters");

        RuleFor(x => x.Version)
            .GreaterThan(0).WithMessage("Version must be greater than zero");

        RuleFor(x => x.MaxAmountOfCandidatesRestriction)
            .GreaterThan(0).WithMessage("Max amount of candidates must be greater than zero")
            .LessThanOrEqualTo(1000).WithMessage("Max amount of candidates cannot exceed 1000");

        RuleFor(x => x.MinimumRequirements)
            .NotEmpty().WithMessage("Minimum requirements are required")
            .Must(requirements => requirements.Count > 0).WithMessage("At least one minimum requirement is required");

        RuleFor(x => x.ExperienceLevel)
            .NotEmpty().WithMessage("Experience level is required")
            .Must(level => new[] { "Entry", "Mid", "Senior", "Lead", "Executive" }.Contains(level))
            .WithMessage("Experience level must be: Entry, Mid, Senior, Lead, or Executive");

        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(200).WithMessage("Job title cannot exceed 200 characters");

        RuleFor(x => x.JobType)
            .NotEmpty().WithMessage("Job type is required")
            .Must(type => new[] { "FullTime", "PartTime", "Contract", "Internship" }.Contains(type))
            .WithMessage("Job type must be: FullTime, PartTime, Contract, or Internship");

        RuleFor(x => x.JobDescription)
            .NotEmpty().WithMessage("Job description is required")
            .MaximumLength(2000).WithMessage("Job description cannot exceed 2000 characters");
    }
}
