using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Scoring.Dto;
using Recruiter.Application.Scoring.Interfaces;
using Ardalis.Result;

namespace Recruiter.Application.Scoring;

public class ScoringService : IScoringService
{
    private readonly IRepository<Domain.Models.Scoring> _repository;
    private readonly IMapper _mapper;

    public ScoringService(IRepository<Domain.Models.Scoring> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<ScoringDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<ScoringDto>>(entities);
    }

    public async Task<ScoringDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<ScoringDto>(entity) : null;
    }

    public async Task<ScoringDto> CreateAsync(ScoringDto dto)
    {
        var entity = _mapper.Map<Domain.Models.Scoring>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ScoringDto>(entity);
    }

    public async Task<ScoringDto> UpdateAsync(ScoringDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"Scoring with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ScoringDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity != null)
        {
            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null;
    }

    public async Task<Result<List<ScoringDto>>> GetByCvEvaluationIdAsync(Guid cvEvaluationId, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(cancellationToken);
        var filtered = entities.Where(e => e.CvEvaluationId == cvEvaluationId).ToList();
        var dtos = _mapper.Map<List<ScoringDto>>(filtered);
        return Result<List<ScoringDto>>.Success(dtos);
    }
}
