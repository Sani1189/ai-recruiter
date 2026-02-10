using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

/// <summary>
/// Gets a template by name and version without tracking for read-only versioning operations.
/// Using AsNoTracking prevents EF Core tracking conflicts when reading existing data to create new versions.
/// </summary>
public sealed class QuestionnaireTemplateByNameAndVersionNoTrackingSpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateByNameAndVersionNoTrackingSpec(string name, int version)
    {
        Query
            .AsNoTracking()
            .Where(x => x.Name == name && x.Version == version && !x.IsDeleted)
            .Include(x => x.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Questions.Where(q => q.IsActive).OrderBy(q => q.Order))
                    .ThenInclude(q => q.Options.OrderBy(o => o.Order));
    }
}
