using FluentValidation;
using Recruiter.Application.Comment.Dto;

namespace Recruiter.Application.Comment.Validation;

public class CommentListQueryDtoValidator : AbstractValidator<CommentListQueryDto>
{
    public CommentListQueryDtoValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage("Invalid sort field. Valid fields are: Id, Content, EntityId, EntityType, CreatedAt, UpdatedAt.");

        RuleFor(x => x.CreatedBefore)
            .GreaterThan(x => x.CreatedAfter)
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue)
            .WithMessage("CreatedBefore must be after CreatedAfter.");
    }

    private bool BeValidSortField(string? sortField)
    {
        if (string.IsNullOrWhiteSpace(sortField))
            return true;

        var validFields = new[] { "id", "content", "entityid", "entitytype", "createdat", "updatedat" };
        return validFields.Contains(sortField.ToLower());
    }
}
