using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Education.Dto;
using Recruiter.Application.Education.Interfaces;
using Recruiter.Application.Education.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.Education;

public class EducationService : IEducationService
{
    private readonly IRepository<Domain.Models.Education> _repository;
    private readonly IMapper _mapper;

    public EducationService(IRepository<Domain.Models.Education> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<EducationDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<EducationDto>>(entities);
    }

    public async Task<EducationDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<EducationDto>(entity) : null;
    }

    public async Task<EducationDto> CreateAsync(EducationDto dto)
    {
        var entity = _mapper.Map<Domain.Models.Education>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<EducationDto>(entity);
    }

    public async Task<EducationDto> UpdateAsync(EducationDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"Education with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<EducationDto>(entity);
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

    public async Task<Result<List<EducationDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new EducationByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<EducationDto>>(entities);
        return Result<List<EducationDto>>.Success(dtos);
    }
}
