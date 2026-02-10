using Recruiter.Application.KeyStrength.Dto;
using Ardalis.Result;

namespace Recruiter.Application.KeyStrength.Interfaces;

public interface IKeyStrengthService
{
    Task<IEnumerable<KeyStrengthDto>> GetAllAsync();
    Task<KeyStrengthDto?> GetByIdAsync(Guid id);
    Task<KeyStrengthDto> CreateAsync(KeyStrengthDto dto);
    Task<KeyStrengthDto> UpdateAsync(KeyStrengthDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<KeyStrengthDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
