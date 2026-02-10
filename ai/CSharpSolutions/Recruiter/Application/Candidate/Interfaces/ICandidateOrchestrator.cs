using Ardalis.Result;
using Recruiter.Application.Candidate.Dto;

namespace Recruiter.Application.Candidate.Interfaces;

// Candidate Orchestrator for complex business operations
public interface ICandidateOrchestrator
{
    Task<Result<CandidateDto>> GetCandidateWithProfileAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CandidateDto>> ProcessCandidateAsync(CandidateDto candidateDto, CancellationToken cancellationToken = default);
    Task<Result<CandidateDto>> UpdateCandidateProfileAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}