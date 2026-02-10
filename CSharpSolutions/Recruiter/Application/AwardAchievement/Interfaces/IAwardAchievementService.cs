using Recruiter.Application.AwardAchievement.Dto;
using Ardalis.Result;

namespace Recruiter.Application.AwardAchievement.Interfaces;

public interface IAwardAchievementService
{
    Task<IEnumerable<AwardAchievementDto>> GetAllAsync();
    Task<AwardAchievementDto?> GetByIdAsync(Guid id);
    Task<AwardAchievementDto> CreateAsync(AwardAchievementDto dto);
    Task<AwardAchievementDto> UpdateAsync(AwardAchievementDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<AwardAchievementDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
