using FluentValidation;
using Recruiter.Application.JobApplicationStepFiles.Dto;

namespace Recruiter.Application.JobApplicationStepFiles.Validation;

public class JobApplicationStepFilesDtoValidator : AbstractValidator<JobApplicationStepFilesDto>
{
    public JobApplicationStepFilesDtoValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID is required.");

        RuleFor(x => x.JobApplicationStepId)
            .NotEmpty().WithMessage("Job application step ID is required.");
    }
}
