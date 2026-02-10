using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.ProjectResearch.Dto;
using Recruiter.Application.ProjectResearch.Interfaces;
using Recruiter.Application.ProjectResearch.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.ProjectResearch;

public class ProjectResearchService : IProjectResearchService
{
    private readonly IRepository<Domain.Models.ProjectResearch> _repository;
    private readonly IMapper _mapper;

    public ProjectResearchService(IRepository<Domain.Models.ProjectResearch> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<ProjectResearchDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<ProjectResearchDto>>(entities);
    }

    public async Task<ProjectResearchDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<ProjectResearchDto>(entity) : null;
    }

    public async Task<ProjectResearchDto> CreateAsync(ProjectResearchDto dto)
    {
        var entity = _mapper.Map<Domain.Models.ProjectResearch>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ProjectResearchDto>(entity);
    }

    public async Task<ProjectResearchDto> UpdateAsync(ProjectResearchDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"ProjectResearch with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ProjectResearchDto>(entity);
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

    public async Task<Result<List<ProjectResearchDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new ProjectResearchByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<ProjectResearchDto>>(entities);
        return Result<List<ProjectResearchDto>>.Success(dtos);
    }
}
