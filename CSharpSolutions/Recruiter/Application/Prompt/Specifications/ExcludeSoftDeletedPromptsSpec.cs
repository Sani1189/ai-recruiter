using Ardalis.Specification;

namespace Recruiter.Application.Prompt.Queries;

public sealed class ExcludeSoftDeletedPromptsSpec : Specification<Domain.Models.Prompt>
{
    public ExcludeSoftDeletedPromptsSpec()
    {
        Query.Where(p => !p.IsDeleted);
    }
}