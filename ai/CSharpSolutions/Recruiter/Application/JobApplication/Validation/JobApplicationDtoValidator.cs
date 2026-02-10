using FluentValidation;
using Recruiter.Application.JobApplication.Dto;

namespace Recruiter.Application.JobApplication.Validation;

public class JobApplicationDtoValidator : AbstractValidator<JobApplicationDto>
{
    public JobApplicationDtoValidator()
    {
        RuleFor(x => x.JobPostName)
            .NotEmpty()
            .WithMessage("Job Post Name is required.");

        RuleFor(x => x.JobPostVersion)
            .GreaterThan(0)
            .WithMessage("Job Post Version must be greater than 0.");

        RuleFor(x => x.CandidateId)
            .NotEmpty()
            .WithMessage("Candidate ID is required.");
    }
}
