using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class OptionsLatestBatchSpec : Specification<QuestionnaireQuestionOption>
{
    public OptionsLatestBatchSpec(IEnumerable<string> optionNames)
    {
        var names = optionNames.Distinct().ToList();
        if (!names.Any())
        {
            Query.Where(o => false);
            return;
        }

        Query
            .Where(o => names.Contains(o.Name) && !o.IsDeleted)
            .OrderBy(o => o.Name)
            .ThenByDescending(o => o.Version);
    }
}
