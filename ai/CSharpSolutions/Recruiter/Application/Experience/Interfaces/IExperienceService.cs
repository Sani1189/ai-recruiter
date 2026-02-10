using Recruiter.Application.Experience.Dto;
using Ardalis.Result;

namespace Recruiter.Application.Experience.Interfaces;

public interface IExperienceService
{
    Task<IEnumerable<ExperienceDto>> GetAllAsync();
    Task<ExperienceDto?> GetByIdAsync(Guid id);
    Task<ExperienceDto> CreateAsync(ExperienceDto dto);
    Task<ExperienceDto> UpdateAsync(ExperienceDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<ExperienceDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
