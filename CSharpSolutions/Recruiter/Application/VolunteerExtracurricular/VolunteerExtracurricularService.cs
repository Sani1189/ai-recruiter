using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.VolunteerExtracurricular.Dto;
using Recruiter.Application.VolunteerExtracurricular.Interfaces;
using Recruiter.Application.VolunteerExtracurricular.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.VolunteerExtracurricular;

public class VolunteerExtracurricularService : IVolunteerExtracurricularService
{
    private readonly IRepository<Domain.Models.VolunteerExtracurricular> _repository;
    private readonly IMapper _mapper;

    public VolunteerExtracurricularService(IRepository<Domain.Models.VolunteerExtracurricular> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<VolunteerExtracurricularDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<VolunteerExtracurricularDto>>(entities);
    }

    public async Task<VolunteerExtracurricularDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<VolunteerExtracurricularDto>(entity) : null;
    }

    public async Task<VolunteerExtracurricularDto> CreateAsync(VolunteerExtracurricularDto dto)
    {
        var entity = _mapper.Map<Domain.Models.VolunteerExtracurricular>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<VolunteerExtracurricularDto>(entity);
    }

    public async Task<VolunteerExtracurricularDto> UpdateAsync(VolunteerExtracurricularDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"VolunteerExtracurricular with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<VolunteerExtracurricularDto>(entity);
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

    public async Task<Result<List<VolunteerExtracurricularDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new VolunteerExtracurricularByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<VolunteerExtracurricularDto>>(entities);
        return Result<List<VolunteerExtracurricularDto>>.Success(dtos);
    }
}
