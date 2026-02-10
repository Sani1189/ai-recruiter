using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Experience.Dto;
using Recruiter.Application.Experience.Interfaces;
using Recruiter.Application.Experience.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.Experience;

public class ExperienceService : IExperienceService
{
    private readonly IRepository<Domain.Models.Experience> _repository;
    private readonly IMapper _mapper;

    public ExperienceService(IRepository<Domain.Models.Experience> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<ExperienceDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<ExperienceDto>>(entities);
    }

    public async Task<ExperienceDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<ExperienceDto>(entity) : null;
    }

    public async Task<ExperienceDto> CreateAsync(ExperienceDto dto)
    {
        var entity = _mapper.Map<Domain.Models.Experience>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ExperienceDto>(entity);
    }

    public async Task<ExperienceDto> UpdateAsync(ExperienceDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"Experience with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ExperienceDto>(entity);
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

    public async Task<Result<List<ExperienceDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new ExperienceByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<ExperienceDto>>(entities);
        return Result<List<ExperienceDto>>.Success(dtos);
    }
}
