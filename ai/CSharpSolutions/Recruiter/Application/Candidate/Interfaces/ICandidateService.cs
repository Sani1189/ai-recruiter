using Recruiter.Application.Candidate.Dto;
using Ardalis.Result;

namespace Recruiter.Application.Candidate.Interfaces;

public interface ICandidateService
{
    // Basic CRUD operations
    Task<IEnumerable<CandidateDto>> GetAllAsync();
    Task<CandidateDto?> GetByIdAsync(Guid id);
    Task<CandidateDto> CreateAsync(CandidateDto dto);
    Task<CandidateDto> UpdateAsync(CandidateDto dto);
    // Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    // Enhanced methods with UserProfile information
    Task<CandidateDto?> GetByIdWithUserProfileAsync(Guid id);
    Task<IEnumerable<CandidateDto>> GetAllWithUserProfileAsync();
    Task<Result<CandidateDto>> GetByUserIdWithUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<List<CandidateDto>>> GetRecentCandidatesWithUserProfileAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<CandidateDto>>> GetFilteredCandidatesWithUserProfileAsync(CandidateListQueryDto query, CancellationToken cancellationToken = default);

    // Legacy methods (kept for backward compatibility)
    Task<Result<List<CandidateDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<CandidateDto>>> GetFilteredCandidatesAsync(CandidateListQueryDto query, CancellationToken cancellationToken = default);
    
    Task<Result<CandidateDto>> GetCandidateDetailsById(Guid id, CancellationToken cancellationToken = default);
}
