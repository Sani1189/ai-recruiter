using Ardalis.Result;
using FluentValidation;
using Recruiter.Application.Candidate.Dto;
using Recruiter.Application.Candidate.Interfaces;

namespace Recruiter.Application.Candidate;



// Candidate Orchestrator implementation
public class CandidateOrchestrator : ICandidateOrchestrator
{
    private readonly ICandidateService _candidateService;
    private readonly IValidator<CandidateDto> _candidateValidator;

    public CandidateOrchestrator(
        ICandidateService candidateService,
        IValidator<CandidateDto> candidateValidator)
    {
        _candidateService = candidateService;
        _candidateValidator = candidateValidator;
    }

    public async Task<Result<CandidateDto>> GetCandidateWithProfileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var candidate = await _candidateService.GetByIdAsync(id);
        if (candidate == null)
        {
            return Result<CandidateDto>.NotFound($"Candidate with ID {id} not found");
        }

        // Get related profile and CV file would be handled here
        // This is a simple implementation - in real scenario you might want to include profile in the DTO

        return Result<CandidateDto>.Success(candidate);
    }

    public async Task<Result<CandidateDto>> ProcessCandidateAsync(CandidateDto candidateDto, CancellationToken cancellationToken = default)
    {
        // Validate the candidate
        var validationResult = await _candidateValidator.ValidateAsync(candidateDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<CandidateDto>.Invalid(validationResult.Errors.Select(e => new ValidationError { ErrorMessage = e.ErrorMessage }).ToArray());
        }

        // Process the candidate (create or update)
        CandidateDto result;
        if (candidateDto.Id == Guid.Empty)
        {
            result = await _candidateService.CreateAsync(candidateDto);
        }
        else
        {
            result = await _candidateService.UpdateAsync(candidateDto);
        }

        return Result<CandidateDto>.Success(result);
    }

    public async Task<Result<CandidateDto>> UpdateCandidateProfileAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var candidate = await _candidateService.GetByIdAsync(id);
        if (candidate == null)
        {
            return Result<CandidateDto>.NotFound($"Candidate with ID {id} not found");
        }

        // Update user ID
        candidate.UserId = userId;
        var updatedCandidate = await _candidateService.UpdateAsync(candidate);

        return Result<CandidateDto>.Success(updatedCandidate);
    }
}
