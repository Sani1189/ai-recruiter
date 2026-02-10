using Ardalis.Specification;
using Recruiter.Domain.Models;
using Recruiter.Application.UserProfile.Dto;

namespace Recruiter.Application.UserProfile.Specifications;

public sealed class UserProfileFilterSpec : Specification<Domain.Models.UserProfile>
{
    public UserProfileFilterSpec(UserProfileListQueryDto query)
    {
        // Apply filters
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

        // Apply sorting
        var sortField = query.SortBy?.ToLower() ?? "createdat";
        
        if (query.SortDescending)
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderByDescending(up => up.Id);
                    break;
                case "name":
                    Query.OrderByDescending(up => up.Name);
                    break;
                case "email":
                    Query.OrderByDescending(up => up.Email);
                    break;
                case "nationality":
                    Query.OrderByDescending(up => up.Nationality);
                    break;
                case "age":
                    Query.OrderByDescending(up => up.Age);
                    break;
                case "updatedat":
                    Query.OrderByDescending(up => up.UpdatedAt);
                    break;
                default:
                    Query.OrderByDescending(up => up.CreatedAt);
                    break;
            }
        }
        else
        {
            switch (sortField)
            {
                case "id":
                    Query.OrderBy(up => up.Id);
                    break;
                case "name":
                    Query.OrderBy(up => up.Name);
                    break;
                case "email":
                    Query.OrderBy(up => up.Email);
                    break;
                case "nationality":
                    Query.OrderBy(up => up.Nationality);
                    break;
                case "age":
                    Query.OrderBy(up => up.Age);
                    break;
                case "updatedat":
                    Query.OrderBy(up => up.UpdatedAt);
                    break;
                default:
                    Query.OrderBy(up => up.CreatedAt);
                    break;
            }
        }

        // Apply pagination
        Query.Skip((query.PageNumber - 1) * query.PageSize)
             .Take(query.PageSize);
    }
}
