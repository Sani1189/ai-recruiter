using Ardalis.Specification;

namespace Recruiter.Application.Scoring.Specifications;

/// <summary>
/// Filters scoring rows by CV evaluation id.
/// </summary>
public sealed class ScoringByCvEvaluationIdSpec : Specification<Domain.Models.Scoring>
{
    public ScoringByCvEvaluationIdSpec(Guid cvEvaluationId)
    {
        Query.Where(s => s.CvEvaluationId == cvEvaluationId && s.IsDeleted == false);
    }
}

