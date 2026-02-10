using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionnaireTemplateLatestByNameSpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateLatestByNameSpec(string name)
    {
        Query
            .Where(x => x.Name == name && !x.IsDeleted)
            .OrderByDescending(x => x.Version)
            .Take(1)
            .Include(x => x.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Questions.Where(q => q.IsActive).OrderBy(q => q.Order))
                    .ThenInclude(q => q.Options.OrderBy(o => o.Order));
    }
}


