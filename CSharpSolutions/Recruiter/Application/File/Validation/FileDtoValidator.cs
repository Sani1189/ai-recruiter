using FluentValidation;
using Recruiter.Application.File.Dto;

namespace Recruiter.Application.File.Validation;

public class FileDtoValidator : AbstractValidator<FileDto>
{
    public FileDtoValidator()
    {
        RuleFor(x => x.Container)
            .NotEmpty()
            .WithMessage("Container is required.");

        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage("File path is required.");

        RuleFor(x => x.Extension)
            .NotEmpty()
            .WithMessage("Extension is required.");

        RuleFor(x => x.MbSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0.");
    }
}
