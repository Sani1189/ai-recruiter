using Ardalis.Specification;
using Recruiter.Domain.Models;

namespace Recruiter.Application.InterviewConfiguration.Specifications;

public sealed class InterviewConfigurationsByModalitySpec : Specification<Domain.Models.InterviewConfiguration>
{
    public InterviewConfigurationsByModalitySpec(string modality)
    {
        Query.Where(ic => ic.Modality == modality)
             .OrderByDescending(ic => ic.CreatedAt);
    }
}
