using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Interview.Dto;
using Recruiter.Application.Interview.Interfaces;
using Recruiter.Domain.Models;
using Ardalis.Result;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.Interview;

public class InterviewService : IInterviewService
{
    private readonly IRepository<Domain.Models.Interview> _repository;
    private readonly IRepository<Domain.Models.JobApplicationStep> _jobApplicationStepRepository;
    private readonly IRepository<Domain.Models.JobApplication> _jobApplicationRepository;
    private readonly IRepository<Domain.Models.JobPostStep> _jobPostStepRepository;
    private readonly InterviewConfiguration.Queries.InterviewConfigurationQueryHandler _configQueryHandler;
    private readonly IMapper _mapper;
    private readonly Queries.InterviewQueryHandler _queryHandler;

    public InterviewService(IRepository<Domain.Models.Interview> repository, IMapper mapper, Queries.InterviewQueryHandler queryHandler, IRepository<Domain.Models.JobApplicationStep> jobApplicationStepRepository, IRepository<Domain.Models.JobPostStep> jobPostStepRepository, InterviewConfiguration.Queries.InterviewConfigurationQueryHandler configQueryHandler, IRepository<Domain.Models.JobApplication> jobApplicationRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _jobApplicationStepRepository = jobApplicationStepRepository ?? throw new ArgumentNullException(nameof(jobApplicationStepRepository));
        _jobPostStepRepository = jobPostStepRepository ?? throw new ArgumentNullException(nameof(jobPostStepRepository));
        _configQueryHandler = configQueryHandler ?? throw new ArgumentNullException(nameof(configQueryHandler));
        _jobApplicationRepository = jobApplicationRepository ?? throw new ArgumentNullException(nameof(jobApplicationRepository));
    }

    public async Task<IEnumerable<InterviewDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<InterviewDto>>(entities);
    }

    public async Task<InterviewDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<InterviewDto>(entity) : null;
    }

    public async Task<InterviewDto> CreateAsync(InterviewDto dto)
    {
        var entity = _mapper.Map<Domain.Models.Interview>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InterviewDto>(entity);
    }

    public async Task<InterviewDto> UpdateAsync(InterviewDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"Interview with id '{dto.Id}' not found.");
        }

        // Map DTO to entity
        var updatedEntity = _mapper.Map<Domain.Models.Interview>(dto);
        updatedEntity.Id = entity.Id; // Preserve the ID

        await _repository.UpdateAsync(updatedEntity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InterviewDto>(updatedEntity);
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

    // Delegate complex queries to QueryHandler
    public async Task<Result<List<InterviewDto>>> GetByJobApplicationStepIdAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByJobApplicationStepIdAsync(jobApplicationStepId, cancellationToken);
    }

    public async Task<Result<List<InterviewDto>>> GetByConfigurationAsync(string configName, int configVersion, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByConfigurationAsync(configName, configVersion, cancellationToken);
    }

    public async Task<Result<Common.Dto.PagedResult<InterviewDto>>> GetFilteredInterviewsAsync(InterviewListQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredInterviewsAsync(query, cancellationToken);
    }

    public async Task<Result<InterviewDto>> CompleteInterviewAsync(Guid interviewId, Dto.CompleteInterviewDto completeDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(interviewId);
        if (entity == null)
            return Result<InterviewDto>.NotFound();

        entity.CompletedAt = DateTimeOffset.UtcNow;

         // Calculate duration in milliseconds from timestamps
        if (entity.StartedAt.HasValue)
        {
            var totalMs = (long)(entity.CompletedAt.Value - entity.StartedAt.Value).TotalMilliseconds;
            entity.Duration = Math.Max(1, totalMs);
        }
        else if (completeDto.Duration.HasValue)
        {
            entity.Duration = completeDto.Duration;
        }
        if (!string.IsNullOrWhiteSpace(completeDto.TranscriptUrl)) entity.TranscriptUrl = completeDto.TranscriptUrl;
        if (!string.IsNullOrWhiteSpace(completeDto.InterviewAudioUrl)) entity.InterviewAudioUrl = completeDto.InterviewAudioUrl;

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        // Also mark the related step as completed
        var step = await _jobApplicationStepRepository.GetByIdAsync(entity.JobApplicationStepId);
        if (step != null)
        {
            step.Status = JobApplicationStepStatusEnum.Completed;
            step.CompletedAt = DateTimeOffset.UtcNow;
            await _jobApplicationStepRepository.UpdateAsync(step);
            await _jobApplicationStepRepository.SaveChangesAsync();
        }

        // Do NOT mark application Completed automatically here; Finish button will do it explicitly

        var dto = _mapper.Map<InterviewDto>(entity);
        return Result<InterviewDto>.Success(dto);
    }

    public async Task<Result<InterviewDto>> CreateForStepAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default)
    {
        // 0) Idempotency: if an interview already exists for this step, return it
        var existingForStep = await _queryHandler.GetByJobApplicationStepIdAsync(jobApplicationStepId, cancellationToken);
        if (existingForStep.IsSuccess && existingForStep.Value != null && existingForStep.Value.Count > 0)
        {
            // Return the first (by any order) existing interview
            return Result<InterviewDto>.Success(existingForStep.Value.First());
        }

        // 1) Load JobApplicationStep to get JobPostStepName/Version
        var step = await _jobApplicationStepRepository.GetByIdAsync(jobApplicationStepId);
        if (step == null)
        {
            return Result<InterviewDto>.NotFound();
        }

        // 2) Load JobPostStep by name/version; if version is null or 0 (not possible here), fallback to latest
        var jobPostStep = await _jobPostStepRepository.FirstOrDefaultAsync(
            new JobPost.Specifications.JobPostStepByNameAndVersionSpec(step.JobPostStepName, step.JobPostStepVersion),
            cancellationToken
        );
        if (jobPostStep == null)
        {
            // Fallback to latest by name
            jobPostStep = await _jobPostStepRepository.FirstOrDefaultAsync(
                new JobPost.Specifications.JobPostStepLatestByNameSpec(step.JobPostStepName),
                cancellationToken
            );
            if (jobPostStep == null)
                return Result<InterviewDto>.Invalid(new ValidationError { ErrorMessage = "JobPostStep not found" });
        }

        // 3) Resolve InterviewConfiguration name/version
        var configName = jobPostStep.InterviewConfigurationName ?? string.Empty;
        if (string.IsNullOrWhiteSpace(configName))
            return Result<InterviewDto>.Invalid(new ValidationError { ErrorMessage = "InterviewConfigurationName missing on JobPostStep" });

        var configVersion = jobPostStep.InterviewConfigurationVersion;
        var configResult = configVersion.HasValue
            ? await _configQueryHandler.GetByNameAndVersionAsync(configName, configVersion.Value, cancellationToken)
            : await _configQueryHandler.GetLatestByNameAsync(configName, cancellationToken);

        if (!configResult.IsSuccess || configResult.Value == null)
            return Result<InterviewDto>.Invalid(new ValidationError { ErrorMessage = "Interview configuration not found" });

        var config = configResult.Value;

        // 4) Build Interview entity using resolved prompts (all required in Interview)
        var entity = new Domain.Models.Interview
        {
            JobApplicationStepId = jobApplicationStepId,
            InterviewConfigurationName = config.Name!,
            InterviewConfigurationVersion = config.Version,
            InstructionPromptName = config.InstructionPromptName,
            PersonalityPromptName = config.PersonalityPromptName,
            QuestionsPromptName = config.QuestionsPromptName,
            InstructionPromptVersion = config.InstructionPromptVersion ?? 1,
            PersonalityPromptVersion = config.PersonalityPromptVersion ?? 1,
            QuestionsPromptVersion = config.QuestionsPromptVersion ?? 1,
            StartedAt = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        // 5) Ensure JobApplicationStep is marked InProgress
        if (step.Status != Domain.Enums.JobApplicationStepStatusEnum.InProgress)
        {
            step.Status = Domain.Enums.JobApplicationStepStatusEnum.InProgress;
            step.StartedAt = step.StartedAt ?? DateTimeOffset.UtcNow;
            await _jobApplicationStepRepository.UpdateAsync(step);
            await _jobApplicationStepRepository.SaveChangesAsync();
        }

        var dto = _mapper.Map<InterviewDto>(entity);
        return Result<InterviewDto>.Success(dto);
    }
}
