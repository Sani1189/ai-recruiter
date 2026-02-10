using FluentValidation;
using Recruiter.Application.UserProfile.Dto;

namespace Recruiter.Application.UserProfile.Validation;

public class UserProfileDtoValidator : AbstractValidator<UserProfileDto>
{
    public UserProfileDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid email address is required.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithMessage("Phone number cannot exceed 20 characters.");

        RuleFor(x => x.Nationality)
            .MaximumLength(100)
            .WithMessage("Nationality cannot exceed 100 characters.");

        RuleFor(x => x.ProfilePictureUrl)
            .MaximumLength(500)
            .WithMessage("Profile picture URL cannot exceed 500 characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(18, 100)
            .When(x => x.Age.HasValue)
            .WithMessage("Age must be between 18 and 100 when provided.");
    }
}
