using Ardalis.Result;
using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Interview.Interfaces;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.JobApplication.Specifications;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplication;

/// <summary>
/// Candidate-focused orchestration for the job application flow:
/// - Targeted lookup by job post
/// - Progress snapshot (application + steps)
/// - Idempotent step begin (create/get application + step, start, ensure interview if needed)
/// </summary>
public sealed class JobApplicationCandidateFlowService : IJobApplicationCandidateFlowService
{
    private readonly IJobApplicationService _jobApplicationService;
    private readonly IRepository<JobApplicationStep> _stepRepository;
    private readonly IRepository<JobPostStep> _jobPostStepRepository;
    private readonly IInterviewService _interviewService;
    private readonly IMapper _mapper;

    public JobApplicationCandidateFlowService(
        IJobApplicationService jobApplicationService,
        IRepository<JobApplicationStep> stepRepository,
        IRepository<JobPostStep> jobPostStepRepository,
        IInterviewService interviewService,
        IMapper mapper)
    {
        _jobApplicationService = jobApplicationService ?? throw new ArgumentNullException(nameof(jobApplicationService));
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _jobPostStepRepository = jobPostStepRepository ?? throw new ArgumentNullException(nameof(jobPostStepRepository));
        _interviewService = interviewService ?? throw new ArgumentNullException(nameof(interviewService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<JobApplicationDto?>> GetMyApplicationByJobPostAsync(
        string jobPostName,
        int jobPostVersion,
        CancellationToken cancellationToken = default)
        => await _jobApplicationService.GetMyApplicationByJobPostAsync(jobPostName, jobPostVersion, cancellationToken);

    public async Task<Result<MyJobApplicationProgressDto>> GetMyProgressAsync(
        string jobPostName,
        int jobPostVersion,
        CancellationToken cancellationToken = default)
    {
        var appResult = await _jobApplicationService.GetMyApplicationByJobPostAsync(jobPostName, jobPostVersion, cancellationToken);
        if (!appResult.IsSuccess)
        {
            return TranslateResult<JobApplicationDto?, MyJobApplicationProgressDto>(appResult);
        }

        if (appResult.Value?.Id is null || appResult.Value.Id == Guid.Empty)
        {
            return Result<MyJobApplicationProgressDto>.Success(new MyJobApplicationProgressDto
            {
                JobApplication = null,
                Steps = new List<JobApplicationStepDto>()
            });
        }

        var appId = appResult.Value.Id.Value;
        var stepSpec = new JobApplicationStepByJobApplicationIdSpec(appId);
        var steps = await _stepRepository.ListAsync(stepSpec, cancellationToken);

        var stepDtos = _mapper.Map<List<JobApplicationStepDto>>(steps.OrderBy(s => s.StepNumber));

        return Result<MyJobApplicationProgressDto>.Success(new MyJobApplicationProgressDto
        {
            JobApplication = appResult.Value,
            Steps = stepDtos
        });
    }

    public async Task<Result<BeginJobApplicationStepResponseDto>> BeginStepAsync(
        string jobPostName,
        int jobPostVersion,
        BeginJobApplicationStepRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return Result<BeginJobApplicationStepResponseDto>.Invalid(new ValidationError { ErrorMessage = "Request is required" });
        }

        if (string.IsNullOrWhiteSpace(request.StepName))
        {
            return Result<BeginJobApplicationStepResponseDto>.Invalid(new ValidationError { ErrorMessage = "StepName is required" });
        }

        if (request.StepNumber < 1)
        {
            return Result<BeginJobApplicationStepResponseDto>.Invalid(new ValidationError { ErrorMessage = "StepNumber must be greater than 0" });
        }

        // 1) Ensure JobApplication exists only at the moment a step begins.
        var appResult = await _jobApplicationService.CreateOrGetJobApplicationAsync(jobPostName, jobPostVersion, null, cancellationToken);
        if (!appResult.IsSuccess || appResult.Value?.Id is null || appResult.Value.Id == Guid.Empty)
        {
            return TranslateResult<JobApplicationDto, BeginJobApplicationStepResponseDto>(appResult);
        }

        var appId = appResult.Value.Id.Value;

        // 2) Resolve step version (nullable means "latest")
        var resolvedStepVersion = request.StepVersion;
        if (!resolvedStepVersion.HasValue || resolvedStepVersion.Value < 1)
        {
            var latest = await _jobPostStepRepository.FirstOrDefaultAsync(
                new JobPostStepLatestByNameSpec(request.StepName),
                cancellationToken);

            if (latest == null)
            {
                return Result<BeginJobApplicationStepResponseDto>.NotFound($"JobPostStep '{request.StepName}' was not found.");
            }

            resolvedStepVersion = latest.Version;
        }

        // 3) Idempotently create/get step record for this application+jobPostStep
        var existingStep = await _stepRepository.FirstOrDefaultAsync(
            new JobApplicationStepByAppAndJobPostStepSpec(appId, request.StepName, resolvedStepVersion.Value),
            cancellationToken);

        JobApplicationStep stepEntity;
        if (existingStep != null)
        {
            stepEntity = existingStep;
        }
        else
        {
            stepEntity = new JobApplicationStep
            {
                JobApplicationId = appId,
                JobPostStepName = request.StepName,
                JobPostStepVersion = resolvedStepVersion.Value,
                StepNumber = request.StepNumber,
                Status = JobApplicationStepStatusEnum.Pending
            };

            await _stepRepository.AddAsync(stepEntity, cancellationToken);
        }

        // 4) Mark as started (InProgress) if not already
        if (stepEntity.Status != JobApplicationStepStatusEnum.InProgress)
        {
            stepEntity.Status = JobApplicationStepStatusEnum.InProgress;
        }

        stepEntity.StartedAt ??= DateTimeOffset.UtcNow;

        await _stepRepository.UpdateAsync(stepEntity, cancellationToken);
        await _stepRepository.SaveChangesAsync(cancellationToken);

        // 5) If interview step, ensure interview exists (idempotent)
        Guid? interviewId = null;
        var jobPostStep = await _jobPostStepRepository.FirstOrDefaultAsync(
            new JobPostStepByNameAndVersionSpec(request.StepName, resolvedStepVersion.Value),
            cancellationToken);

        if (jobPostStep?.IsInterview == true)
        {
            var interviewResult = await _interviewService.CreateForStepAsync(stepEntity.Id, cancellationToken);
            if (interviewResult.IsSuccess && interviewResult.Value?.Id is { } intId)
            {
                interviewId = intId;
            }
        }

        return Result<BeginJobApplicationStepResponseDto>.Success(new BeginJobApplicationStepResponseDto
        {
            JobApplicationId = appId,
            JobApplicationStepId = stepEntity.Id,
            InterviewId = interviewId,
            StepStatus = stepEntity.Status.ToString()
        });
    }

    private static Result<TTo> TranslateResult<TFrom, TTo>(Result<TFrom> source)
    {
        // Map status without leaking unrelated generic types
        return source.Status switch
        {
            ResultStatus.Unauthorized => Result<TTo>.Unauthorized(),
            ResultStatus.Forbidden => Result<TTo>.Forbidden(),
            ResultStatus.NotFound => Result<TTo>.NotFound(),
            ResultStatus.Invalid => Result<TTo>.Invalid(source.ValidationErrors.ToArray()),
            _ => Result<TTo>.Error()
        };
    }
}


