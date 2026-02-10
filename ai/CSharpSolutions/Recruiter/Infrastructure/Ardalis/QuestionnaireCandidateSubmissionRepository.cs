using Microsoft.EntityFrameworkCore;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

/// <summary>
/// Specialized repository for QuestionnaireCandidateSubmission to handle optimistic concurrency gracefully.
/// Submissions can have concurrent updates (e.g., when candidate submits while system processes).
///
/// Strategy: retry on DbUpdateConcurrencyException by refreshing original values from DB
/// (last-write-wins). If still failing, bubble up so middleware returns 409.
/// </summary>
public sealed class QuestionnaireCandidateSubmissionRepository : EfRepository<QuestionnaireCandidateSubmission>
{
    private readonly RecruiterDbContext _context;

    public QuestionnaireCandidateSubmissionRepository(RecruiterDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Submissions can have concurrent updates. Retry with refresh of RowVersion values.
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxAttempts)
            {
                // Refresh ORIGINAL values for *all* modified/deleted tracked entities that have RowVersion,
                // not only those included in ex.Entries. This prevents a "second conflict" on retry when the
                // first conflict is resolved but another tracked row was also updated concurrently.
                // CRITICAL: We only refresh Modified/Deleted entities - Added entities (like new answers) MUST be preserved.
                var candidates = _context.ChangeTracker
                    .Entries()
                    .Where(e =>
                        (e.State == EntityState.Modified || e.State == EntityState.Deleted) &&
                        e.Metadata.FindProperty("RowVersion") != null)
                    .ToList();

                // If EF didn't track anything with RowVersion, fall back to just the entries in the exception.
                if (candidates.Count == 0)
                {
                    candidates = ex.Entries.ToList();
                }

                foreach (var entry in candidates)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                    // If row was deleted by another process
                    if (databaseValues == null)
                    {
                        if (entry.State == EntityState.Deleted)
                        {
                            entry.State = EntityState.Detached;
                            continue;
                        }

                        // Real not-found or conflicting delete/update - bubble up
                        throw;
                    }

                    // CRITICAL: Only refresh the RowVersion property, NOT all original values
                    // This preserves the navigation properties (Answers collection) and ensures Added child entities remain tracked
                    var rowVersionProperty = entry.Metadata.FindProperty("RowVersion");
                    if (rowVersionProperty != null && databaseValues.Properties.Contains(rowVersionProperty))
                    {
                        entry.OriginalValues[rowVersionProperty] = databaseValues[rowVersionProperty];
                    }
                    else
                    {
                        // Fallback: refresh all original values if RowVersion property not found
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                }

                // Tiny backoff to reduce thrashing under rapid concurrent updates
                await Task.Delay(TimeSpan.FromMilliseconds(25 * attempt), cancellationToken);
            }
        }

        // Unreachable, but required for compiler
        return await base.SaveChangesAsync(cancellationToken);
    }
}
