using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.InterviewConfiguration.Dto;
using Recruiter.Application.InterviewConfiguration.Interfaces;
using Recruiter.Application.InterviewConfiguration.Specifications;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Application.Prompt.Interfaces;
using Recruiter.Application.Prompt.Specifications;
using Recruiter.Domain.Models;
using Ardalis.Result;

namespace Recruiter.Application.InterviewConfiguration;

public class InterviewConfigurationService : IInterviewConfigurationService
{
    private readonly IRepository<Domain.Models.InterviewConfiguration> _repository;
    private readonly IPromptRepository _promptRepository;
    private readonly IRepository<JobPostStep> _jobPostStepRepository;
    private readonly IRepository<Domain.Models.Interview> _interviewRepository;
    private readonly IMapper _mapper;
    private readonly Queries.InterviewConfigurationQueryHandler _queryHandler;

    public InterviewConfigurationService(
        IRepository<Domain.Models.InterviewConfiguration> repository, 
        IPromptRepository promptRepository,
        IRepository<JobPostStep> jobPostStepRepository,
        IRepository<Domain.Models.Interview> interviewRepository,
        IMapper mapper, 
        Queries.InterviewConfigurationQueryHandler queryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _promptRepository = promptRepository ?? throw new ArgumentNullException(nameof(promptRepository));
        _jobPostStepRepository = jobPostStepRepository ?? throw new ArgumentNullException(nameof(jobPostStepRepository));
        _interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    public async Task<IEnumerable<InterviewConfigurationDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<InterviewConfigurationDto>>(entities);
    }

    public async Task<InterviewConfigurationDto?> GetByIdAsync(string name, int version)
    {
        var entity = await _repository.FirstOrDefaultAsync(new InterviewConfigurationByNameAndVersionSpec(name, version));
        return entity != null ? _mapper.Map<InterviewConfigurationDto>(entity) : null;
    }

    public async Task<InterviewConfigurationDto?> GetLatestVersionAsync(string name)
    {
        var entity = await _repository.FirstOrDefaultAsync(new InterviewConfigurationLatestByNameSpec(name));
        return entity != null ? _mapper.Map<InterviewConfigurationDto>(entity) : null;
    }

    public async Task<IEnumerable<InterviewConfigurationDto>> GetAllVersionsAsync(string name)
    {
        var entities = await _repository.ListAsync(new InterviewConfigurationsByNameSpec(name));
        return _mapper.Map<IEnumerable<InterviewConfigurationDto>>(entities);
    }

    public async Task<InterviewConfigurationDto> CreateAsync(InterviewConfigurationDto dto)
    {
        var entity = _mapper.Map<Domain.Models.InterviewConfiguration>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InterviewConfigurationDto>(entity);
    }

    public async Task<InterviewConfigurationDto> UpdateAsync(InterviewConfigurationDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Get existing configuration by name + version
        var existingEntity = await _repository.FirstOrDefaultAsync(
            new InterviewConfigurationByNameAndVersionSpec(dto.Name, dto.Version)
        );

        if (existingEntity == null)
        throw new InvalidOperationException($"InterviewConfiguration with name '{dto.Name}' and version '{dto.Version}' not found.");

        Domain.Models.InterviewConfiguration targetEntity;

        if (dto.ShouldUpdateVersion == true)
        {
            // Fetch the latest version for this name (for safe increment)
            var latestEntity = await _repository.FirstOrDefaultAsync(
                new InterviewConfigurationLatestByNameSpec(dto.Name)
            );

            var newVersion = (latestEntity?.Version ?? dto.Version) + 1;

            targetEntity = new Domain.Models.InterviewConfiguration
            {
                Name = dto.Name,
                Version = newVersion,
                Modality = dto.Modality,
                Tone = dto.Tone,
                ProbingDepth = dto.ProbingDepth,
                FocusArea = dto.FocusArea,
                Duration = dto.Duration,
                Language = dto.Language,
                InstructionPromptName = dto.InstructionPromptName,
                InstructionPromptVersion = dto.InstructionPromptVersion,
                PersonalityPromptName = dto.PersonalityPromptName,
                PersonalityPromptVersion = dto.PersonalityPromptVersion,
                QuestionsPromptName = dto.QuestionsPromptName,
                QuestionsPromptVersion = dto.QuestionsPromptVersion,
                Active = dto.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _repository.AddAsync(targetEntity);
        }
        else
        {
            // Update existing (Name and Version remain unchanged)
            existingEntity.Modality = dto.Modality;
            existingEntity.Tone = dto.Tone;
            existingEntity.ProbingDepth = dto.ProbingDepth;
            existingEntity.FocusArea = dto.FocusArea;
            existingEntity.Duration = dto.Duration;
            existingEntity.Language = dto.Language;
            existingEntity.InstructionPromptName = dto.InstructionPromptName;
            existingEntity.InstructionPromptVersion = dto.InstructionPromptVersion;
            existingEntity.PersonalityPromptName = dto.PersonalityPromptName;
            existingEntity.PersonalityPromptVersion = dto.PersonalityPromptVersion;
            existingEntity.QuestionsPromptName = dto.QuestionsPromptName;
            existingEntity.QuestionsPromptVersion = dto.QuestionsPromptVersion;
            existingEntity.Active = dto.Active;
            existingEntity.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.UpdateAsync(existingEntity);
            targetEntity = existingEntity;
        }

        await _repository.SaveChangesAsync();

        return _mapper.Map<InterviewConfigurationDto>(targetEntity);
    }

    public async Task DeleteAsync(string name, int version)
    {
        var entity = await _repository.FirstOrDefaultAsync(new InterviewConfigurationByNameAndVersionSpec(name, version));
        if (entity == null)
            return;

        // Check if interview configuration is in use
        bool isInUse = await IsInterviewConfigurationInUseAsync(name, version);

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
    }

    public async Task<bool> IsInterviewConfigurationInUseAsync(string name, int version)
    {
        // Check if used in JobPostStep
        var usedInJobPostStepCount = await _jobPostStepRepository.CountAsync(
            new JobPostStepsUsingInterviewConfigurationSpec(name, version));

        if (usedInJobPostStepCount > 0)
            return true;

        // Dynamic references (InterviewConfigurationVersion == null => "use latest") should block deleting the latest version
        var latest = await GetLatestVersionAsync(name);
        if (latest != null && latest.Version == version)
        {
            var dynamicJobPostStepCount = await _jobPostStepRepository.CountAsync(
                new JobPostStepsUsingInterviewConfigurationNameWithDynamicLatestSpec(name));

            if (dynamicJobPostStepCount > 0)
                return true;
        }

        // Check if used in Interview
        var usedInInterviewCount = await _interviewRepository.CountAsync(
            new InterviewsUsingInterviewConfigurationSpec(name, version));

        return usedInInterviewCount > 0;
    }

    public async Task RestoreInterviewConfigurationAsync(string name, int version)
    {
        // Use IgnoreQueryFilters to get even soft-deleted records
        var entity = await _repository.FirstOrDefaultAsync(new InterviewConfigurationByNameAndVersionSpec(name, version));
        
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
        var entity = await _repository.FirstOrDefaultAsync(new InterviewConfigurationByNameAndVersionSpec(name, version));
        return entity != null;
    }

    // Delegate complex queries to QueryHandler
    public async Task<Result<InterviewConfigurationDto>> GetLatestByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetLatestByNameAsync(name, cancellationToken);
    }

    public async Task<Result<List<InterviewConfigurationDto>>> GetByModalityAsync(string modality, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByModalityAsync(modality, cancellationToken);
    }

    public async Task<Result<List<InterviewConfigurationDto>>> GetActiveConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetActiveConfigurationsAsync(cancellationToken);
    }

    public async Task<Result<Common.Dto.PagedResult<InterviewConfigurationDto>>> GetFilteredConfigurationsAsync(InterviewConfigurationListQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredConfigurationsAsync(query, cancellationToken);
    }

    public async Task<InterviewConfigurationDto?> DuplicateAsync(string sourceName, int sourceVersion, DuplicateInterviewConfigurationRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.NewName)) throw new InvalidOperationException("New interview configuration name is required.");

        var source = await _repository.FirstOrDefaultAsync(new InterviewConfigurationByNameAndVersionSpec(sourceName, sourceVersion));
        if (source == null)
        {
            return null;
        }

        var newName = request.NewName.Trim();

        // Ensure target name is not already in use (any version, non-deleted)
        var existingTarget = await _repository.FirstOrDefaultAsync(new InterviewConfigurationLatestByNameSpec(newName));
        if (existingTarget != null)
        {
            throw new InvalidOperationException($"An InterviewConfiguration with name '{newName}' already exists.");
        }

        // Duplicate as a fresh v1 with identical settings/prompts; only Name changes.
        var duplicated = new Domain.Models.InterviewConfiguration
        {
            Name = newName,
            Version = 1,
            Modality = source.Modality,
            Tone = source.Tone,
            ProbingDepth = source.ProbingDepth,
            FocusArea = source.FocusArea,
            Duration = source.Duration,
            Language = source.Language,
            InstructionPromptName = source.InstructionPromptName,
            InstructionPromptVersion = source.InstructionPromptVersion,
            PersonalityPromptName = source.PersonalityPromptName,
            PersonalityPromptVersion = source.PersonalityPromptVersion,
            QuestionsPromptName = source.QuestionsPromptName,
            QuestionsPromptVersion = source.QuestionsPromptVersion,
            Active = source.Active,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(duplicated);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InterviewConfigurationDto>(duplicated);
    }

    // New methods for prompt resolution and validation
    public async Task<InterviewConfigurationWithPromptsDto?> GetWithResolvedPromptsAsync(string name, int version)
    {
        var config = await _repository.FirstOrDefaultAsync(new InterviewConfigurationByNameAndVersionSpec(name, version));
        if (config == null)
            return null;

        return await MapToWithPromptsDtoAsync(config);
    }

    public async Task<InterviewConfigurationWithPromptsDto?> GetLatestWithResolvedPromptsAsync(string name)
    {
        var config = await _repository.FirstOrDefaultAsync(new InterviewConfigurationLatestByNameSpec(name));
        if (config == null)
            return null;

        return await MapToWithPromptsDtoAsync(config);
    }

    public async Task<List<PromptVersionDto>> GetPromptVersionsAsync(string promptName)
    {
        var prompts = await _promptRepository.ListAsync(new PromptsByNameSpec(promptName));
        return _mapper.Map<List<PromptVersionDto>>(prompts);
    }

    public async Task<bool> ValidatePromptAsync(string promptName, int? version = null)
    {
        if (version.HasValue)
        {
            return await _promptRepository.AnyAsync(new PromptByNameAndVersionSpec(promptName, version.Value));
        }
        else
        {
            return await _promptRepository.AnyAsync(new PromptsByNameSpec(promptName));
        }
    }

    private async Task<InterviewConfigurationWithPromptsDto> MapToWithPromptsDtoAsync(Domain.Models.InterviewConfiguration config)
    {
        var dto = _mapper.Map<InterviewConfigurationWithPromptsDto>(config);
        
        // Resolve prompts (specific version or latest)
        dto.InstructionPrompt = await GetResolvedPromptAsync(config.InstructionPromptName, config.InstructionPromptVersion);
        dto.PersonalityPrompt = await GetResolvedPromptAsync(config.PersonalityPromptName, config.PersonalityPromptVersion);
        dto.QuestionsPrompt = await GetResolvedPromptAsync(config.QuestionsPromptName, config.QuestionsPromptVersion);

        return dto;
    }

    private async Task<PromptDto?> GetResolvedPromptAsync(string promptName, int? version)
    {
        if (version.HasValue)
        {
            // Use specific version
            var prompt = await _promptRepository.FirstOrDefaultAsync(new PromptByNameAndVersionSpec(promptName, version.Value));
            return prompt != null ? _mapper.Map<PromptDto>(prompt) : null;
        }
        else
        {
            // Use latest version
            var prompt = await _promptRepository.FirstOrDefaultAsync(new LatestPromptByNameSpec(promptName));
            return prompt != null ? _mapper.Map<PromptDto>(prompt) : null;
        }
    }
}
