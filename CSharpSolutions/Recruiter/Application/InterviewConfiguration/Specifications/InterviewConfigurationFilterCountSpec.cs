using Ardalis.Specification;
using Recruiter.Domain.Models;
using Recruiter.Application.InterviewConfiguration.Dto;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class InterviewConfigurationFilterCountSpec : Specification<Domain.Models.InterviewConfiguration>
{
    public InterviewConfigurationFilterCountSpec(InterviewConfigurationListQueryDto query)
    {
        // skip isDeleted prompts
        Query.Where(ic => !ic.IsDeleted);
        // Apply only filters (no sorting or pagination for count)
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
    }
}
