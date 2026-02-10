using Recruiter.Application.Education.Dto;
using Ardalis.Result;

namespace Recruiter.Application.Education.Interfaces;

public interface IEducationService
{
    Task<IEnumerable<EducationDto>> GetAllAsync();
    Task<EducationDto?> GetByIdAsync(Guid id);
    Task<EducationDto> CreateAsync(EducationDto dto);
    Task<EducationDto> UpdateAsync(EducationDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<EducationDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
