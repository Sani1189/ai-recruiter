using Recruiter.Application.Dashboard.Dto;
using Ardalis.Result;

namespace Recruiter.Application.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}

