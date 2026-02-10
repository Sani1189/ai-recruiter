using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Specifications;

public sealed class QuestionsLatestBatchSpec : Specification<QuestionnaireQuestion>
{
    public QuestionsLatestBatchSpec(IEnumerable<string> questionNames)
    {
        var names = questionNames.Distinct().ToList();
        if (!names.Any())
        {
            Query.Where(q => false);
            return;
        }

        Query
            .Where(q => names.Contains(q.Name) && !q.IsDeleted)
            .OrderBy(q => q.Name)
            .ThenByDescending(q => q.Version);
    }
}
