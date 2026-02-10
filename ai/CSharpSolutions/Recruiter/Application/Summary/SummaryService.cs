using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Summary.Dto;
using Recruiter.Application.Summary.Interfaces;
using Recruiter.Application.Summary.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.Summary;

public class SummaryService : ISummaryService
{
    private readonly IRepository<Domain.Models.Summary> _repository;
    private readonly IMapper _mapper;

    public SummaryService(IRepository<Domain.Models.Summary> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<SummaryDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<SummaryDto>>(entities);
    }

    public async Task<SummaryDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<SummaryDto>(entity) : null;
    }

    public async Task<SummaryDto> CreateAsync(SummaryDto dto)
    {
        var entity = _mapper.Map<Domain.Models.Summary>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<SummaryDto>(entity);
    }

    public async Task<SummaryDto> UpdateAsync(SummaryDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"Summary with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<SummaryDto>(entity);
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

    public async Task<Result<List<SummaryDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new SummaryByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<SummaryDto>>(entities);
        return Result<List<SummaryDto>>.Success(dtos);
    }
}
