using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.AwardAchievement.Dto;
using Recruiter.Application.AwardAchievement.Interfaces;
using Recruiter.Application.AwardAchievement.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.AwardAchievement;

public class AwardAchievementService : IAwardAchievementService
{
    private readonly IRepository<Domain.Models.AwardAchievement> _repository;
    private readonly IMapper _mapper;

    public AwardAchievementService(IRepository<Domain.Models.AwardAchievement> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<AwardAchievementDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<AwardAchievementDto>>(entities);
    }

    public async Task<AwardAchievementDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<AwardAchievementDto>(entity) : null;
    }

    public async Task<AwardAchievementDto> CreateAsync(AwardAchievementDto dto)
    {
        var entity = _mapper.Map<Domain.Models.AwardAchievement>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<AwardAchievementDto>(entity);
    }

    public async Task<AwardAchievementDto> UpdateAsync(AwardAchievementDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"AwardAchievement with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<AwardAchievementDto>(entity);
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

    public async Task<Result<List<AwardAchievementDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new AwardAchievementByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<AwardAchievementDto>>(entities);
        return Result<List<AwardAchievementDto>>.Success(dtos);
    }
}
