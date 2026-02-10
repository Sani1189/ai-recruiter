using Recruiter.Application.VolunteerExtracurricular.Dto;
using Ardalis.Result;

namespace Recruiter.Application.VolunteerExtracurricular.Interfaces;

public interface IVolunteerExtracurricularService
{
    Task<IEnumerable<VolunteerExtracurricularDto>> GetAllAsync();
    Task<VolunteerExtracurricularDto?> GetByIdAsync(Guid id);
    Task<VolunteerExtracurricularDto> CreateAsync(VolunteerExtracurricularDto dto);
    Task<VolunteerExtracurricularDto> UpdateAsync(VolunteerExtracurricularDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<VolunteerExtracurricularDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
