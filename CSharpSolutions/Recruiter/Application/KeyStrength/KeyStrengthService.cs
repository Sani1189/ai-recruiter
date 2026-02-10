using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.KeyStrength.Dto;
using Recruiter.Application.KeyStrength.Interfaces;
using Recruiter.Application.KeyStrength.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.KeyStrength;

public class KeyStrengthService : IKeyStrengthService
{
    private readonly IRepository<Domain.Models.KeyStrength> _repository;
    private readonly IMapper _mapper;

    public KeyStrengthService(IRepository<Domain.Models.KeyStrength> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<KeyStrengthDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<KeyStrengthDto>>(entities);
    }

    public async Task<KeyStrengthDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<KeyStrengthDto>(entity) : null;
    }

    public async Task<KeyStrengthDto> CreateAsync(KeyStrengthDto dto)
    {
        var entity = _mapper.Map<Domain.Models.KeyStrength>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<KeyStrengthDto>(entity);
    }

    public async Task<KeyStrengthDto> UpdateAsync(KeyStrengthDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"KeyStrength with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<KeyStrengthDto>(entity);
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

    public async Task<Result<List<KeyStrengthDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new KeyStrengthByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<KeyStrengthDto>>(entities);
        return Result<List<KeyStrengthDto>>.Success(dtos);
    }
}
