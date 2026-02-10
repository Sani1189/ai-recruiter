using FluentValidation;
using Recruiter.Application.Candidate.Dto;

namespace Recruiter.Application.Candidate.Validation;

public class CandidateDtoValidator : AbstractValidator<CandidateDto>
{
    public CandidateDtoValidator()
    {
        RuleFor(x => x.CvFileId)
            .NotEmpty()
            .WithMessage("CV File ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
