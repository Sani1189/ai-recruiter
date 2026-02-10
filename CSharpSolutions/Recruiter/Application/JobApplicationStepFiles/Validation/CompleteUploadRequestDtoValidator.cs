using FluentValidation;
using Recruiter.Application.JobApplicationStepFiles.Dto;

namespace Recruiter.Application.JobApplicationStepFiles.Validation;

public class CompleteUploadRequestDtoValidator : AbstractValidator<CompleteUploadRequestDto>
{
    public CompleteUploadRequestDtoValidator()
    {
        RuleFor(x => x.JobPostName)
            .NotEmpty().WithMessage("Job post name is required")
            .MaximumLength(255).WithMessage("Job post name cannot exceed 255 characters");

        RuleFor(x => x.JobPostVersion)
            .GreaterThan(0).WithMessage("Job post version must be greater than 0");

        RuleFor(x => x.StepName)
            .NotEmpty().WithMessage("Step name is required")
            .MaximumLength(255).WithMessage("Step name cannot exceed 255 characters");

        RuleFor(x => x.StepVersion)
            .GreaterThan(0).WithMessage("Step version must be greater than 0")
            .When(x => x.StepVersion.HasValue);

        RuleFor(x => x.BlobPath)
            .NotEmpty().WithMessage("Blob path is required")
            .MaximumLength(1000).WithMessage("Blob path cannot exceed 1000 characters");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty().WithMessage("Original file name is required")
            .MaximumLength(500).WithMessage("Original file name cannot exceed 500 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .MaximumLength(100).WithMessage("Content type cannot exceed 100 characters");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(5 * 1024 * 1024).WithMessage("File size cannot exceed 5MB");
    }
}

