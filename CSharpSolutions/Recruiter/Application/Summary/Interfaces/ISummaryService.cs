using Recruiter.Application.Summary.Dto;
using Ardalis.Result;

namespace Recruiter.Application.Summary.Interfaces;

public interface ISummaryService
{
    Task<IEnumerable<SummaryDto>> GetAllAsync();
    Task<SummaryDto?> GetByIdAsync(Guid id);
    Task<SummaryDto> CreateAsync(SummaryDto dto);
    Task<SummaryDto> UpdateAsync(SummaryDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<SummaryDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
