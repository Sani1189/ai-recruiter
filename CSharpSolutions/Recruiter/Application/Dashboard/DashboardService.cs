using Ardalis.Result;
using Recruiter.Application.Dashboard.Dto;
using Recruiter.Application.Dashboard.Interfaces;
using Recruiter.Application.Dashboard.Queries;

namespace Recruiter.Application.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly DashboardQueryHandler _queryHandler;

    public DashboardService(DashboardQueryHandler queryHandler)
    {
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetDashboardStatsAsync(cancellationToken);
    }
}

