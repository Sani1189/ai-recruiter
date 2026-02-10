using Microsoft.EntityFrameworkCore;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

public sealed class QuestionnaireTemplateRepository : EfVersionedRepository<QuestionnaireTemplate>
{
    private readonly RecruiterDbContext _context;

    public QuestionnaireTemplateRepository(RecruiterDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxAttempts)
            {
                var candidates = _context.ChangeTracker
                    .Entries()
                    .Where(e =>
                        (e.State == EntityState.Modified || e.State == EntityState.Deleted) &&
                        e.Metadata.FindProperty("RowVersion") != null)
                    .ToList();

                if (candidates.Count == 0)
                {
                    candidates = ex.Entries.ToList();
                }

                foreach (var entry in candidates)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                    if (databaseValues == null)
                    {
                        if (entry.State == EntityState.Deleted)
                        {
                            entry.State = EntityState.Detached;
                            continue;
                        }

                        throw;
                    }

                    entry.OriginalValues.SetValues(databaseValues);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex) && attempt < maxAttempts)
            {
                throw;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        var message = ex.Message + (ex.InnerException?.Message ?? string.Empty);
        return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase);
    }
}

