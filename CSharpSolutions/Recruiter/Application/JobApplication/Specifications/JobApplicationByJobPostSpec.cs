using Ardalis.Specification;

namespace Recruiter.Application.JobApplication.Specifications;

public sealed class JobApplicationByJobPostSpec : Specification<Domain.Models.JobApplication>
{
    public JobApplicationByJobPostSpec(string jobPostName, int jobPostVersion)
    {
        Query.Where(ja => ja.JobPostName == jobPostName && ja.JobPostVersion == jobPostVersion)
             .Include(ja => ja.Candidate!)
                .ThenInclude(c => c.CvFile!)
             .Include(ja => ja.Candidate!)   
                .ThenInclude(c => c.UserProfile!)
             .OrderByDescending(ja => ja.CreatedAt);
    }
}
