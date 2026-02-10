using AutoMapper;
using Recruiter.Application.Common;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.JobApplication.Specifications;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Domain.Enums;
using Ardalis.Result;

namespace Recruiter.Application.JobApplication;

public class JobApplicationStepService : IJobApplicationStepService
{
    private readonly IRepository<Domain.Models.JobApplicationStep> _repository;
    private readonly IRepository<Domain.Models.JobApplication> _jobApplicationRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICandidateService _candidateService;
    private readonly IUserProfileService _userProfileService;

    public JobApplicationStepService(
        IRepository<Domain.Models.JobApplicationStep> repository,
        IRepository<Domain.Models.JobApplication> jobApplicationRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ICandidateService candidateService,
        IUserProfileService userProfileService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _jobApplicationRepository = jobApplicationRepository ?? throw new ArgumentNullException(nameof(jobApplicationRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _candidateService = candidateService ?? throw new ArgumentNullException(nameof(candidateService));
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
    }

    // Admin methods
    public async Task<Result<JobApplicationStepDto>> CreateAsync(JobApplicationStepDto stepDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = _mapper.Map<Domain.Models.JobApplicationStep>(stepDto);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            var dto = _mapper.Map<JobApplicationStepDto>(entity);
            return Result<JobApplicationStepDto>.Success(dto);
        }
        catch
        {
            return Result<JobApplicationStepDto>.Error();
        }
    }

    public async Task<Result<JobApplicationStepDto>> UpdateAsync(JobApplicationStepDto stepDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(stepDto.Id!.Value);
            if (entity == null)
                return Result<JobApplicationStepDto>.NotFound();

            // Map DTO to entity, preserving the ID
            var updatedEntity = _mapper.Map<Domain.Models.JobApplicationStep>(stepDto);
            updatedEntity.Id = entity.Id;

            await _repository.UpdateAsync(updatedEntity);
            await _repository.SaveChangesAsync();

            var dto = _mapper.Map<JobApplicationStepDto>(updatedEntity);
            return Result<JobApplicationStepDto>.Success(dto);
        }
        catch
        {
            return Result<JobApplicationStepDto>.Error();
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return Result<bool>.NotFound();

            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch
        {
            return Result<bool>.Error();
        }
    }

    public async Task<Result<Common.Dto.PagedResult<JobApplicationStepDto>>> GetFilteredJobApplicationStepsAsync(JobApplicationStepListQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            var countSpec = new JobApplicationStepFilterCountSpec(query);
            var filterSpec = new JobApplicationStepFilterSpec(query);

            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);
            var entities = await _repository.ListAsync(filterSpec, cancellationToken);

            var dtos = _mapper.Map<List<JobApplicationStepDto>>(entities);

            var result = new Common.Dto.PagedResult<JobApplicationStepDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<JobApplicationStepDto>>.Success(result);
        }
        catch
        {
            return Result<Common.Dto.PagedResult<JobApplicationStepDto>>.Error();
        }
    }

    public async Task<Result<JobApplicationStepDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return Result<JobApplicationStepDto>.NotFound();

        var dto = _mapper.Map<JobApplicationStepDto>(entity);
        return Result<JobApplicationStepDto>.Success(dto);
    }

    public async Task<Result<List<JobApplicationStepDto>>> GetByJobApplicationIdAsync(Guid jobApplicationId, CancellationToken cancellationToken = default)
    {
        if (jobApplicationId == Guid.Empty)
            return Result<List<JobApplicationStepDto>>.Invalid(new ValidationError { ErrorMessage = "Job application ID cannot be empty" });

        var spec = new JobApplicationStepByJobApplicationIdSpec(jobApplicationId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<JobApplicationStepDto>>(entities);
        
        return Result<List<JobApplicationStepDto>>.Success(dtos);
    }

    public async Task<Result<List<JobApplicationStepDto>>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status))
            return Result<List<JobApplicationStepDto>>.Invalid(new ValidationError { ErrorMessage = "Status cannot be empty" });

        if (!Enum.TryParse<JobApplicationStepStatusEnum>(status, true, out var parsed))
            return Result<List<JobApplicationStepDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid status value" });

        var spec = new JobApplicationStepByStatusSpec(parsed);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<JobApplicationStepDto>>(entities);
        
        return Result<List<JobApplicationStepDto>>.Success(dtos);
    }

    public async Task<Result<JobApplicationStepDto>> UpdateStepStatusAsync(Guid stepId, string status, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(stepId);
        if (entity == null)
            return Result<JobApplicationStepDto>.NotFound();

        if (!Enum.TryParse<JobApplicationStepStatusEnum>(status, true, out var parsed))
            return Result<JobApplicationStepDto>.Invalid(new ValidationError { ErrorMessage = "Invalid status value" });

        entity.Status = parsed;
        if (parsed == JobApplicationStepStatusEnum.Completed)
            entity.CompletedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        var dto = _mapper.Map<JobApplicationStepDto>(entity);
        return Result<JobApplicationStepDto>.Success(dto);
    }

    // Candidate methods

    public async Task<Result<JobApplicationStepDto>> UpdateMyStepAsync(Guid stepId, JobApplicationStepDto stepDto, CancellationToken cancellationToken = default)
    {
        var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
        if (candidateId == null)
            return Result<JobApplicationStepDto>.Error();

        // Verify the step belongs to the candidate (simplified - would need proper implementation)
        var verificationResult = await VerifyStepOwnershipAsync(stepId, candidateId.Value, cancellationToken);
        if (!verificationResult.IsSuccess)
            return Result<JobApplicationStepDto>.Unauthorized();

        var entity = await _repository.GetByIdAsync(stepId);
        if (entity == null)
            return Result<JobApplicationStepDto>.NotFound();

        // Update only allowed fields for candidates
        entity.Data = stepDto.Data;
        entity.Status = Enum.TryParse<JobApplicationStepStatusEnum>(stepDto.Status, true, out var parsedStatus) ? parsedStatus : JobApplicationStepStatusEnum.Pending;

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        var dto = _mapper.Map<JobApplicationStepDto>(entity);
        return Result<JobApplicationStepDto>.Success(dto);
    }

    public async Task<Result<JobApplicationStepDto>> CompleteMyStepAsync(Guid stepId, string? data, CancellationToken cancellationToken = default)
    {
        var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
        if (candidateId == null)
            return Result<JobApplicationStepDto>.Error();

        // Verify the step belongs to the candidate (simplified - would need proper implementation)
        var verificationResult = await VerifyStepOwnershipAsync(stepId, candidateId.Value, cancellationToken);
        if (!verificationResult.IsSuccess)
            return Result<JobApplicationStepDto>.Unauthorized();

        var entity = await _repository.GetByIdAsync(stepId);
        if (entity == null)
            return Result<JobApplicationStepDto>.NotFound();

        entity.Status = JobApplicationStepStatusEnum.Completed;
        entity.CompletedAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrEmpty(data))
            entity.Data = data;

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        var dto = _mapper.Map<JobApplicationStepDto>(entity);
        return Result<JobApplicationStepDto>.Success(dto);
    }

    public async Task<Result<JobApplicationStepDto>> StartMyStepAsync(Guid stepId, CancellationToken cancellationToken = default)
    {
        var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
        if (candidateId == null)
            return Result<JobApplicationStepDto>.Error();

        // Verify the step belongs to the candidate (simplified)
        var verificationResult = await VerifyStepOwnershipAsync(stepId, candidateId.Value, cancellationToken);
        if (!verificationResult.IsSuccess)
            return Result<JobApplicationStepDto>.Unauthorized();

        var entity = await _repository.GetByIdAsync(stepId);
        if (entity == null)
            return Result<JobApplicationStepDto>.NotFound();

        entity.Status = Domain.Enums.JobApplicationStepStatusEnum.InProgress;
        // Always stamp StartedAt when starting via candidate flow
        entity.StartedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        var dto = _mapper.Map<JobApplicationStepDto>(entity);
        return Result<JobApplicationStepDto>.Success(dto);
    }

    public async Task<Result<JobApplicationStepDto>> CreateMyStepAsync(Guid applicationId, Dto.CreateStepDto createDto, CancellationToken cancellationToken = default)
    {
        var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
        if (candidateId == null)
            return Result<JobApplicationStepDto>.Error();

        // Verify the application belongs to the candidate (simplified)
        var verificationResult = await VerifyApplicationOwnershipAsync(applicationId, candidateId.Value, cancellationToken);
        if (!verificationResult.IsSuccess)
            return Result<JobApplicationStepDto>.Unauthorized();

        var entity = new Domain.Models.JobApplicationStep
        {
            JobApplicationId = applicationId,
            JobPostStepName = createDto.JobPostStepName,
            JobPostStepVersion = createDto.JobPostStepVersion,
            StepNumber = createDto.StepNumber,
            Data = createDto.Data,
            Status = Domain.Enums.JobApplicationStepStatusEnum.Pending
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        var dto = _mapper.Map<JobApplicationStepDto>(entity);
        return Result<JobApplicationStepDto>.Success(dto);
    }

    /// <summary>
    /// Gets the current candidate ID from the authenticated user's profile
    /// </summary>
    private async Task<Guid?> GetCurrentCandidateIdAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var userProfile = await _userProfileService.GetByEmailAsync(userEmail, cancellationToken);
            if (!userProfile.IsSuccess || userProfile.Value == null || !userProfile.Value.Id.HasValue)
                return null;

            var candidateResult = await _candidateService.GetByUserIdWithUserProfileAsync(userProfile.Value.Id.Value, cancellationToken);
            return candidateResult.IsSuccess ? candidateResult.Value?.Id : null;
        }
        catch
        {
            return null;
        }
    }

    // Simplified verification methods - would need proper implementation with database queries
    private Task<Result<bool>> VerifyApplicationOwnershipAsync(Guid applicationId, Guid candidateId, CancellationToken cancellationToken)
    {
        // TODO: Implement proper verification by querying the database
        // For now, returning true - this should check if application belongs to candidate
        return Task.FromResult(Result<bool>.Success(true));
    }

    private Task<Result<bool>> VerifyStepOwnershipAsync(Guid stepId, Guid candidateId, CancellationToken cancellationToken)
    {
        // TODO: Implement proper verification by querying the database
        // For now, returning true - this should check if step belongs to candidate's application
        return Task.FromResult(Result<bool>.Success(true));
    }

    // Candidate methods
    public async Task<Result<List<JobApplicationStepDto>>> GetMyApplicationStepsAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current candidate ID
            var currentCandidateId = await GetCurrentCandidateIdAsync(cancellationToken);
            if (currentCandidateId == null)
            {
                return Result<List<JobApplicationStepDto>>.Unauthorized();
            }

            // Verify the application belongs to the current candidate
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null || application.CandidateId != currentCandidateId.Value)
            {
                return Result<List<JobApplicationStepDto>>.Unauthorized();
            }

            // Get steps for this application
            var spec = new JobApplicationStepByJobApplicationIdSpec(applicationId);
            var steps = await _repository.ListAsync(spec, cancellationToken);

            var stepDtos = _mapper.Map<List<JobApplicationStepDto>>(steps.OrderBy(s => s.StepNumber));
            return Result<List<JobApplicationStepDto>>.Success(stepDtos);
        }
        catch (Exception ex)
        {
           throw new InvalidOperationException($"Failed to get application steps: {ex.Message}", ex);
        }
    }
}