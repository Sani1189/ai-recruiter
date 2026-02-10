using FluentValidation;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.JobApplication.Validation;

public class JobApplicationStepDtoValidator : AbstractValidator<JobApplicationStepDto>
{
    public JobApplicationStepDtoValidator()
    {
        RuleFor(x => x.JobApplicationId)
            .NotEmpty()
            .WithMessage("Job Application ID is required.");

        RuleFor(x => x.JobPostStepName)
            .NotEmpty()
            .WithMessage("Job Post Step Name is required.");

        RuleFor(x => x.JobPostStepVersion)
            .GreaterThan(0)
            .WithMessage("Job Post Step Version must be greater than 0.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(v => Enum.TryParse<JobApplicationStepStatusEnum>(v, true, out _))
            .WithMessage("Invalid status value.");

        RuleFor(x => x.StepNumber)
            .GreaterThan(0)
            .WithMessage("Step Number must be greater than 0.");
    }
}
