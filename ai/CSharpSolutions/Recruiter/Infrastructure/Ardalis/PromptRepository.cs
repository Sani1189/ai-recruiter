using Recruiter.Application.Prompt.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Recruiter.Infrastructure.Ardalis;

public class PromptRepository : EfVersionedRepository<Prompt>, IPromptRepository
{
    private readonly RecruiterDbContext _context;

    public PromptRepository(RecruiterDbContext context) : base(context)
        => _context = context;

    public async Task<List<string>> GetDistinctCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Prompt>()
            .Where(p => !p.IsDeleted && !string.IsNullOrWhiteSpace(p.Category))
            .Select(p => p.Category.Trim())
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }
}
