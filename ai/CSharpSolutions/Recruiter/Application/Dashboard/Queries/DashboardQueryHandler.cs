using Ardalis.Result;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Dashboard.Dto;
using Recruiter.Application.Dashboard.Specifications;
using DomainModels = Recruiter.Domain.Models;

namespace Recruiter.Application.Dashboard.Queries;

public class DashboardQueryHandler
{
    private readonly IRepository<DomainModels.JobPost> _jobPostRepository;
    private readonly IRepository<DomainModels.Candidate> _candidateRepository;
    private readonly IRepository<DomainModels.Interview> _interviewRepository;

    public DashboardQueryHandler(
        IRepository<DomainModels.JobPost> jobPostRepository,
        IRepository<DomainModels.Candidate> candidateRepository,
        IRepository<DomainModels.Interview> interviewRepository)
    {
        _jobPostRepository = jobPostRepository ?? throw new ArgumentNullException(nameof(jobPostRepository));
        _candidateRepository = candidateRepository ?? throw new ArgumentNullException(nameof(candidateRepository));
        _interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var totalJobOpenings = await _jobPostRepository.CountAsync(new ActiveJobPostsSpec(), cancellationToken);
            var totalCandidates = await _candidateRepository.CountAsync(new ActiveCandidatesSpec(), cancellationToken);
            var interviews = await _interviewRepository.ListAsync(new ActiveInterviewsSpec(), cancellationToken);

            var interviewsWithDuration = interviews?.Where(i => i.Duration.HasValue && i.Duration.Value > 0).ToList() ?? new List<DomainModels.Interview>();
            var avgDurationMinutes = interviewsWithDuration.Any()
                ? interviewsWithDuration.Average(i => i.Duration!.Value / 60000.0)
                : 0.0;

            return Result<DashboardStatsDto>.Success(new DashboardStatsDto
            {
                TotalJobOpenings = totalJobOpenings,
                TotalCandidates = totalCandidates,
                AvgOverallScore = 0.0,
                AvgDurationMinutes = Math.Round(avgDurationMinutes, 2)
            });
        }
        catch (Exception)
        {
            return Result<DashboardStatsDto>.Error();
        }
    }
}

