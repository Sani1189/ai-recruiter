using Ardalis.Specification;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationByJobPostAndCandidateSpec : Specification<Domain.Models.JobApplication>, ISingleResultSpecification<Domain.Models.JobApplication>
{
    public JobApplicationByJobPostAndCandidateSpec(string jobPostName, int jobPostVersion, Guid candidateId)
    {
        Query.Where(ja => ja.JobPostName == jobPostName 
                       && ja.JobPostVersion == jobPostVersion 
                       && ja.CandidateId == candidateId)
             .OrderByDescending(ja => ja.CreatedAt);
    }
}

