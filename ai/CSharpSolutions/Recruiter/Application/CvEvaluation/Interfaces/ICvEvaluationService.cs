using Recruiter.Application.CvEvaluation.Dto;
using Ardalis.Result;

namespace Recruiter.Application.CvEvaluation.Interfaces;

public interface ICvEvaluationService
{
    Task<IEnumerable<CvEvaluationDto>> GetAllAsync();
    Task<CvEvaluationDto?> GetByIdAsync(Guid id);
    Task<CvEvaluationDto> CreateAsync(CvEvaluationDto dto);
    Task<CvEvaluationDto> UpdateAsync(CvEvaluationDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<CvEvaluationDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
