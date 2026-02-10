using Ardalis.Specification;
using Recruiter.Domain.Models;
using Recruiter.Application.UserProfile.Dto;

namespace Recruiter.Application.UserProfile.Specifications;

public sealed class UserProfileFilterCountSpec : Specification<Domain.Models.UserProfile>
{
    public UserProfileFilterCountSpec(UserProfileListQueryDto query)
    {
        // Apply only filters (no sorting or pagination for count)
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(up => up.Name.ToLower().Contains(query.SearchTerm.ToLower()) || 
                             up.Email.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            Query.Where(up => up.Name.ToLower().Contains(query.Name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            Query.Where(up => up.Email.ToLower().Contains(query.Email.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.Nationality))
        {
            Query.Where(up => up.Nationality == query.Nationality);
        }

        if (query.MinAge.HasValue)
        {
            Query.Where(up => up.Age >= query.MinAge.Value);
        }

        if (query.MaxAge.HasValue)
        {
            Query.Where(up => up.Age <= query.MaxAge.Value);
        }

        if (query.OpenToRelocation.HasValue)
        {
            Query.Where(up => up.OpenToRelocation == query.OpenToRelocation.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.JobTypePreference))
        {
            Query.Where(up => up.JobTypePreferences.Contains(query.JobTypePreference));
        }

        if (!string.IsNullOrWhiteSpace(query.RemotePreference))
        {
            Query.Where(up => up.RemotePreferences.Contains(query.RemotePreference));
        }

        if (query.CreatedAfter.HasValue)
        {
            Query.Where(up => up.CreatedAt >= query.CreatedAfter.Value);
        }

        if (query.CreatedBefore.HasValue)
        {
            Query.Where(up => up.CreatedAt <= query.CreatedBefore.Value);
        }

        if (query.IsRecent.HasValue && query.IsRecent.Value)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            Query.Where(up => up.CreatedAt >= cutoffDate);
        }
    }
}
