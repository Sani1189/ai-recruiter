using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Application.Prompt.Interfaces;
using Recruiter.Application.Prompt.Specifications;
using Recruiter.Domain.Models;
using Ardalis.Result;

namespace Recruiter.Application.Prompt;

public class PromptService : IPromptService
{
    private static readonly HashSet<string> ProtectedPromptNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CVExtractionSystemInstructions",
        "CVExtractionScoringInstructions"
    };

    private readonly IRepository<Recruiter.Domain.Models.Prompt> _repository;
    private readonly IRepository<Recruiter.Domain.Models.InterviewConfiguration> _interviewConfigRepository;
    private readonly IRepository<Recruiter.Domain.Models.Interview> _interviewRepository;
    private readonly IMapper _mapper;
    private readonly Queries.PromptQueryHandler _queryHandler;

    public PromptService(
        IRepository<Recruiter.Domain.Models.Prompt> repository, 
        IRepository<Recruiter.Domain.Models.InterviewConfiguration> interviewConfigRepository,
        IRepository<Recruiter.Domain.Models.Interview> interviewRepository,
        IMapper mapper, 
        Queries.PromptQueryHandler queryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _interviewConfigRepository = interviewConfigRepository ?? throw new ArgumentNullException(nameof(interviewConfigRepository));
        _interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    public async Task<Result<List<PromptDto>>> GetAllAsync()
    {
        return await _queryHandler.GetAllAsync();
    }

    public async Task<Result<List<string>>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetCategoriesAsync(cancellationToken);
    }

    public async Task<PromptDto?> GetByIdAsync(string name, int version)
    {
        var entity = await _repository.FirstOrDefaultAsync(new PromptByNameAndVersionSpec(name, version));
        return entity != null ? _mapper.Map<PromptDto>(entity) : null;
    }

    public async Task<PromptDto?> GetLatestVersionAsync(string name)
    {
        var entity = await _repository.FirstOrDefaultAsync(new PromptLatestByNameSpec(name));
        return entity != null ? _mapper.Map<PromptDto>(entity) : null;
    }

    public async Task<IEnumerable<PromptDto>> GetAllVersionsAsync(string name)
    {
        var entities = await _repository.ListAsync(new PromptsByNameSpec(name));
        return _mapper.Map<IEnumerable<PromptDto>>(entities);
    }

    public async Task<PromptDto> CreateAsync(PromptDto dto)
    {
        var entity = _mapper.Map<Domain.Models.Prompt>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<PromptDto>(entity);
    }

    public async Task<PromptDto> UpdateAsync(PromptDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Get existing prompt by name + version
        var existingEntity = await _repository.FirstOrDefaultAsync(
            new PromptByNameAndVersionSpec(dto.Name, dto.Version)
        );

        if (existingEntity == null)
            throw new InvalidOperationException($"Prompt with name '{dto.Name}' and version '{dto.Version}' not found.");

        Domain.Models.Prompt targetEntity;

        if (dto.ShouldUpdateVersion == true)
        {
            // Fetch the latest version for this name (for safe increment)
            var latestEntity = await _repository.FirstOrDefaultAsync(
                new LatestPromptByNameSpec(dto.Name)
            );

            var newVersion = (latestEntity?.Version ?? dto.Version) + 1;

            targetEntity = new Domain.Models.Prompt
            {
                Name = dto.Name,
                Version = newVersion,
                Category = dto.Category,
                Content = dto.Content,
                Locale = dto.Locale,
                Tags = dto.Tags,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _repository.AddAsync(targetEntity);
        }
        else
        {
            // Update existing
            existingEntity.Category = dto.Category;
            existingEntity.Content = dto.Content;
            existingEntity.Locale = dto.Locale;
            existingEntity.Tags = dto.Tags;
            existingEntity.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.UpdateAsync(existingEntity);
            targetEntity = existingEntity;
        }

        await _repository.SaveChangesAsync();

        return _mapper.Map<PromptDto>(targetEntity);
    }

    public async Task<PromptDto?> DuplicateAsync(string sourceName, int sourceVersion, DuplicatePromptRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.NewName)) throw new InvalidOperationException("New prompt name is required.");

        if (ProtectedPromptNames.Contains(sourceName))
        {
            throw new InvalidOperationException($"Prompt '{sourceName}' is protected and cannot be duplicated.");
        }

        var source = await _repository.FirstOrDefaultAsync(new PromptByNameAndVersionSpec(sourceName, sourceVersion));
        if (source == null)
        {
            return null;
        }

        // Reuse CreateAsync semantics (v1) and keep the duplication logic minimal.
        var dto = _mapper.Map<PromptDto>(source);
        dto.Name = request.NewName.Trim();
        dto.Version = 1;
        dto.ShouldUpdateVersion = false;

        return await CreateAsync(dto);
    }


    public async Task<Result> DeleteAsync(string name, int version)
    {
        var entity = await _repository.FirstOrDefaultAsync(new PromptByNameAndVersionSpec(name, version));
        if (entity == null)
            return Result.NotFound();

        // Check if prompt is in use
        bool isInUse = await IsPromptInUseAsync(name, version);

        if (isInUse)
        {
            // Soft delete - mark as deleted
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(entity);
        }
        else
        {
            // Hard delete - remove from database
            await _repository.DeleteAsync(entity);
        }

        await _repository.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<bool> IsPromptInUseAsync(string name, int version)
    {
        // Explicit version references
        var usedInConfigCount = await _interviewConfigRepository.CountAsync(
            new InterviewConfigurationsUsingPromptSpec(name, version));
        if (usedInConfigCount > 0)
            return true;

        var usedInInterviewCount = await _interviewRepository.CountAsync(
            new InterviewsUsingPromptSpec(name, version));
        if (usedInInterviewCount > 0)
            return true;

        // Dynamic references (PromptVersion == null => "use latest") should block deleting the latest version
        var latest = await GetLatestVersionAsync(name);
        if (latest != null && latest.Version == version)
        {
            var dynamicConfigCount = await _interviewConfigRepository.CountAsync(
                new InterviewConfigurationsUsingPromptNameWithDynamicLatestSpec(name));
            if (dynamicConfigCount > 0)
                return true;
        }

        return false;
    }

    public async Task RestorePromptAsync(string name, int version)
    {
        var entity = await _repository.FirstOrDefaultAsync(new PromptByNameAndVersionSpec(name, version));
        
        if (entity != null && entity.IsDeleted)
        {
            entity.IsDeleted = false;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string name, int version)
    {
        var entity = await _repository.FirstOrDefaultAsync(new PromptByNameAndVersionSpec(name, version));
        return entity != null;
    }

    // Delegate complex queries to QueryHandler
    public async Task<Result<PromptDto>> GetLatestByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetLatestByNameAsync(name, cancellationToken);
    }

    public async Task<Result<List<PromptDto>>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByCategoryAsync(category, cancellationToken);
    }

    public async Task<Result<List<PromptDto>>> GetByLocaleAsync(string locale, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByLocaleAsync(locale, cancellationToken);
    }

    public async Task<Result<Common.Dto.PagedResult<PromptDto>>> GetFilteredPromptsAsync(PromptListQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredPromptsAsync(query, cancellationToken);
    }
}
