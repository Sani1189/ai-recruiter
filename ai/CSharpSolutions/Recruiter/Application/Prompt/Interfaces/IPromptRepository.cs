using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt.Interfaces;

public interface IPromptRepository : IRepository<Domain.Models.Prompt>
{
    Task<List<string>> GetDistinctCategoriesAsync(CancellationToken cancellationToken = default);
}
