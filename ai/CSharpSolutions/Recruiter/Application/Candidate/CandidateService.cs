using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Candidate.Dto;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Domain.Models;
using Ardalis.Result;

namespace Recruiter.Application.Candidate;

public class CandidateService : ICandidateService
{
    private readonly ICandidateRepository _repository;
    private readonly IMapper _mapper;
    private readonly Queries.CandidateQueryHandler _queryHandler;

    public CandidateService(ICandidateRepository repository, IMapper mapper, Queries.CandidateQueryHandler queryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    public async Task<IEnumerable<CandidateDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<CandidateDto>>(entities);
    }

    public async Task<CandidateDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<CandidateDto>(entity) : null;
    }

    public async Task<CandidateDto> CreateAsync(CandidateDto dto)
    {
        // Check if a candidate with the same UserId already exists
        if (dto.UserId != Guid.Empty)
        {
            var existingCandidateResult = await GetByUserIdAsync(dto.UserId);
            if (existingCandidateResult.IsSuccess && existingCandidateResult.Value != null && existingCandidateResult.Value.Count > 0)
            {
                throw new InvalidOperationException($"A Candidate with UserId '{dto.UserId}' already exists. One UserProfile can only have one Candidate.");
            }
        }

        var entity = _mapper.Map<Domain.Models.Candidate>(dto);
        
        // Generate CandidateId before saving
        entity.CandidateId = await GenerateCandidateIdAsync();
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<CandidateDto>(entity);
    }

    public async Task<CandidateDto> UpdateAsync(CandidateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"Candidate with id '{dto.Id}' not found.");
        }

        // Map DTO to entity
        var updatedEntity = _mapper.Map<Domain.Models.Candidate>(dto);
        updatedEntity.Id = entity.Id; // Preserve the ID

        await _repository.UpdateAsync(updatedEntity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<CandidateDto>(updatedEntity);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null;
    }

    // Delegate complex queries to QueryHandler
    public async Task<Result<List<CandidateDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByUserIdAsync(userId, cancellationToken);
    }
    public async Task<Result<Common.Dto.PagedResult<CandidateDto>>> GetFilteredCandidatesAsync(CandidateListQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredCandidatesAsync(query, cancellationToken);
    }

    // Enhanced methods with UserProfile information
    public async Task<CandidateDto?> GetByIdWithUserProfileAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return null;

        return _mapper.Map<CandidateDto>(entity);
    }

    public async Task<IEnumerable<CandidateDto>> GetAllWithUserProfileAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<CandidateDto>>(entities);
    }

    public async Task<Result<CandidateDto>> GetByUserIdWithUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByUserIdWithUserProfileAsync(userId, cancellationToken);
    }

    public async Task<Result<List<CandidateDto>>> GetRecentCandidatesWithUserProfileAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetRecentCandidatesWithUserProfileAsync(days, cancellationToken);
    }

    public async Task<Result<Common.Dto.PagedResult<CandidateDto>>> GetFilteredCandidatesWithUserProfileAsync(CandidateListQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredCandidatesWithUserProfileAsync(query, cancellationToken);
    }

    public async Task<Result<CandidateDto>> GetCandidateDetailsById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetCandidateDetailsById(id, cancellationToken);
    }

    private async Task<string> GenerateCandidateIdAsync()
    {
        var now = DateTime.UtcNow;
        var year = now.Year.ToString("00").Substring(2); // Get last 2 digits of year
        var month = now.Month.ToString("00");
        
        // Get count of candidates created in current year and month
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        
        var existingCandidates = await _repository.ListAsync();
        var count = existingCandidates.Count(c => 
            c.CreatedAt >= startOfMonth && 
            c.CreatedAt < endOfMonth);
        
        // Increment count for new candidate
        var nextCount = count + 1;
        
        return $"CA-{year}-{month}-{nextCount:D4}";
    }
}
