using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Loads a template for submission processing.
/// Unlike the candidate/edit read model, this includes BOTH active and inactive question versions so that
/// a candidate can still submit the exact (Name, Version) they received earlier even if a recruiter
/// versioned the question meanwhile (question-only versioning flips IsActive).
/// </summary>
public sealed class QuestionnaireTemplateByNameAndVersionForSubmissionSpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateByNameAndVersionForSubmissionSpec(string name, int version)
    {
        Query
            // IMPORTANT:
            // Soft-deleted templates must still be loadable for submission processing because they may be referenced by
            // existing submissions and represent the immutable version that the candidate received.
            .Where(x => x.Name == name && x.Version == version)
            .Include(x => x.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Questions.OrderBy(q => q.Order))
                    .ThenInclude(q => q.Options.OrderBy(o => o.Order));
    }
}

