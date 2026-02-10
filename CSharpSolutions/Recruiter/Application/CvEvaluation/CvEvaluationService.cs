using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.CvEvaluation.Dto;
using Recruiter.Application.CvEvaluation.Interfaces;
using Recruiter.Application.CvEvaluation.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.CvEvaluation;

public class CvEvaluationService : ICvEvaluationService
{
    private readonly IRepository<Domain.Models.CvEvaluation> _repository;
    private readonly IMapper _mapper;

    public CvEvaluationService(IRepository<Domain.Models.CvEvaluation> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<CvEvaluationDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<CvEvaluationDto>>(entities);
    }

    public async Task<CvEvaluationDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<CvEvaluationDto>(entity) : null;
    }

    public async Task<CvEvaluationDto> CreateAsync(CvEvaluationDto dto)
    {
        var entity = _mapper.Map<Domain.Models.CvEvaluation>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<CvEvaluationDto>(entity);
    }

    public async Task<CvEvaluationDto> UpdateAsync(CvEvaluationDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"CvEvaluation with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<CvEvaluationDto>(entity);
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

    public async Task<Result<List<CvEvaluationDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new CvEvaluationByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<CvEvaluationDto>>(entities);
        return Result<List<CvEvaluationDto>>.Success(dtos);
    }
}
