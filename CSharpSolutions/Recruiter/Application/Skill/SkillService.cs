using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Skill.Dto;
using Recruiter.Application.Skill.Interfaces;
using Recruiter.Application.Skill.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.Skill;

public class SkillService : ISkillService
{
    private readonly IRepository<Domain.Models.Skill> _repository;
    private readonly IMapper _mapper;

    public SkillService(IRepository<Domain.Models.Skill> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<SkillDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<SkillDto>>(entities);
    }

    public async Task<SkillDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<SkillDto>(entity) : null;
    }

    public async Task<SkillDto> CreateAsync(SkillDto dto)
    {
        var entity = _mapper.Map<Domain.Models.Skill>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<SkillDto>(entity);
    }

    public async Task<SkillDto> UpdateAsync(SkillDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"Skill with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<SkillDto>(entity);
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

    public async Task<Result<List<SkillDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new SkillByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<SkillDto>>(entities);
        return Result<List<SkillDto>>.Success(dtos);
    }
}
