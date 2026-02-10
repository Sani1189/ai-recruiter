using FluentValidation;
using Recruiter.Application.Comment.Dto;

namespace Recruiter.Application.Comment.Validation;

public class CommentDtoValidator : AbstractValidator<CommentDto>
{
    public CommentDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required.")
            .MaximumLength(2000)
            .WithMessage("Comment content cannot exceed 2000 characters.");

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("Entity ID is required.");

        RuleFor(x => x.EntityType)
            .IsInEnum()
            .WithMessage("Invalid entity type.");
    }
}
