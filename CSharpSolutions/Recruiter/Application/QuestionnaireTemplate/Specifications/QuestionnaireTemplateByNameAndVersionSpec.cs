using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionnaireTemplateByNameAndVersionSpec : Specification<Domain.Models.QuestionnaireTemplate>
{
    public QuestionnaireTemplateByNameAndVersionSpec(string name, int version)
    {
        Query
            .Where(x => x.Name == name && x.Version == version && !x.IsDeleted)
            .Include(x => x.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Questions.Where(q => q.IsActive).OrderBy(q => q.Order))
                    .ThenInclude(q => q.Options.OrderBy(o => o.Order));
    }
}


