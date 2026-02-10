using Recruiter.Application.Skill.Dto;
using Ardalis.Result;

namespace Recruiter.Application.Skill.Interfaces;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> GetAllAsync();
    Task<SkillDto?> GetByIdAsync(Guid id);
    Task<SkillDto> CreateAsync(SkillDto dto);
    Task<SkillDto> UpdateAsync(SkillDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<SkillDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
