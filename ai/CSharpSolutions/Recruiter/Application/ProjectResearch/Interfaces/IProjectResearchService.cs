using Recruiter.Application.ProjectResearch.Dto;
using Ardalis.Result;

namespace Recruiter.Application.ProjectResearch.Interfaces;

public interface IProjectResearchService
{
    Task<IEnumerable<ProjectResearchDto>> GetAllAsync();
    Task<ProjectResearchDto?> GetByIdAsync(Guid id);
    Task<ProjectResearchDto> CreateAsync(ProjectResearchDto dto);
    Task<ProjectResearchDto> UpdateAsync(ProjectResearchDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<ProjectResearchDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
