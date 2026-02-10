using Ardalis.Specification;
using Recruiter.Domain.Models;
using Recruiter.Application.InterviewConfiguration.Dto;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class InterviewConfigurationFilterSpec : Specification<Domain.Models.InterviewConfiguration>
{
    public InterviewConfigurationFilterSpec(InterviewConfigurationListQueryDto query)
    {
        // skip isDeleted prompts
        Query.Where(ic => !ic.IsDeleted);
        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            Query.Where(ic =>
                ic.Name.Contains(query.SearchTerm) ||
                ic.Modality.Contains(query.SearchTerm) ||
                (ic.Tone != null && ic.Tone.Contains(query.SearchTerm)) ||
                (ic.FocusArea != null && ic.FocusArea.Contains(query.SearchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(query.Modality))
        {
            Query.Where(ic => ic.Modality == query.Modality);
        }

        if (!string.IsNullOrWhiteSpace(query.Tone))
        {
            Query.Where(ic => ic.Tone == query.Tone);
        }

        if (!string.IsNullOrWhiteSpace(query.ProbingDepth))
        {
            Query.Where(ic => ic.ProbingDepth == query.ProbingDepth);
        }

        if (!string.IsNullOrWhiteSpace(query.FocusArea))
        {
            Query.Where(ic => ic.FocusArea == query.FocusArea);
        }

        if (!string.IsNullOrWhiteSpace(query.Language))
        {
            Query.Where(ic => ic.Language == query.Language);
        }

        if (query.Active.HasValue)
        {
            Query.Where(ic => ic.Active == query.Active.Value);
        }

        if (query.CreatedAfter.HasValue)
        {
            Query.Where(ic => ic.CreatedAt >= query.CreatedAfter.Value);
        }

        if (query.CreatedBefore.HasValue)
        {
            Query.Where(ic => ic.CreatedAt <= query.CreatedBefore.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            switch (query.SortBy.ToLower())
            {
                case "name":
                    if (query.SortDescending)
                        Query.OrderByDescending(ic => ic.Name);
                    else
                        Query.OrderBy(ic => ic.Name);
                    break;
                case "version":
                    if (query.SortDescending)
                        Query.OrderByDescending(ic => ic.Version);
                    else
                        Query.OrderBy(ic => ic.Version);
                    break;
                case "modality":
                    if (query.SortDescending)
                        Query.OrderByDescending(ic => ic.Modality);
                    else
                        Query.OrderBy(ic => ic.Modality);
                    break;
                case "duration":
                    if (query.SortDescending)
                        Query.OrderByDescending(ic => ic.Duration);
                    else
                        Query.OrderBy(ic => ic.Duration);
                    break;
                case "createdat":
                default:
                    if (query.SortDescending)
                        Query.OrderByDescending(ic => ic.CreatedAt);
                    else
                        Query.OrderBy(ic => ic.CreatedAt);
                    break;
            }
        }
        else
        {
            Query.OrderByDescending(ic => ic.CreatedAt);
        }

        // Apply pagination
        Query.Skip((query.PageNumber - 1) * query.PageSize)
             .Take(query.PageSize);
    }
}
