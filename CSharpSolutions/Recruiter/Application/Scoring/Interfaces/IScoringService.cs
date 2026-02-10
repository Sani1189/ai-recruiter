using Recruiter.Application.Scoring.Dto;
using Ardalis.Result;

namespace Recruiter.Application.Scoring.Interfaces;

public interface IScoringService
{
    Task<IEnumerable<ScoringDto>> GetAllAsync();
    Task<ScoringDto?> GetByIdAsync(Guid id);
    Task<ScoringDto> CreateAsync(ScoringDto dto);
    Task<ScoringDto> UpdateAsync(ScoringDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<ScoringDto>>> GetByCvEvaluationIdAsync(Guid cvEvaluationId, CancellationToken cancellationToken = default);
}
