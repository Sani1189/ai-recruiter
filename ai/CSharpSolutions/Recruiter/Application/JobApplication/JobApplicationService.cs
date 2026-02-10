using AutoMapper;
using Microsoft.Extensions.Logging;
using Recruiter.Application.Common;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.JobApplication.Specifications;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Domain.Models;
using Ardalis.Result;

namespace Recruiter.Application.JobApplication;

public class JobApplicationService : IJobApplicationService
{
    private readonly IRepository<Domain.Models.JobApplication> _repository;
    private readonly IRepository<Domain.Models.JobApplicationStep> _jobApplicationStepRepository;
    private readonly IRepository<Domain.Models.Interview> _interviewRepository;
    private readonly IRepository<Domain.Models.Candidate> _candidateRepository;
    private readonly IRepository<Domain.Models.JobPostStepAssignment> _jobPostStepAssignmentRepository;
    private readonly IMapper _mapper;
    private readonly Queries.JobApplicationQueryHandler _queryHandler;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICandidateService _candidateService;
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<JobApplicationService> _logger;

    public JobApplicationService(
        IRepository<Domain.Models.JobApplication> repository, 
        IRepository<Domain.Models.JobApplicationStep> jobApplicationStepRepository,
        IRepository<Domain.Models.Interview> interviewRepository,
        IRepository<Domain.Models.Candidate> candidateRepository,
        IRepository<Domain.Models.JobPostStepAssignment> jobPostStepAssignmentRepository,
        IMapper mapper, 
        Queries.JobApplicationQueryHandler queryHandler,
        ICurrentUserService currentUserService,
        ICandidateService candidateService,
        IUserProfileService userProfileService,
        ILogger<JobApplicationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _jobApplicationStepRepository = jobApplicationStepRepository ?? throw new ArgumentNullException(nameof(jobApplicationStepRepository));
        _interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
        _candidateRepository = candidateRepository ?? throw new ArgumentNullException(nameof(candidateRepository));
        _jobPostStepAssignmentRepository = jobPostStepAssignmentRepository ?? throw new ArgumentNullException(nameof(jobPostStepAssignmentRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _candidateService = candidateService ?? throw new ArgumentNullException(nameof(candidateService));
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Admin methods
    public async Task<Result<Common.Dto.PagedResult<JobApplicationDto>>> GetFilteredJobApplicationsAsync(JobApplicationListQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredJobApplicationsAsync(query, cancellationToken);
    }

    public async Task<Result<JobApplicationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return Result<JobApplicationDto>.NotFound();

        var dto = _mapper.Map<JobApplicationDto>(entity);
        return Result<JobApplicationDto>.Success(dto);
    }

    public async Task<Result<List<JobApplicationDto>>> GetByCandidateIdAsync(Guid candidateId, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByCandidateIdAsync(candidateId, cancellationToken);
    }

    public async Task<Result<List<JobApplicationDto>>> GetByJobPostAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByJobPostAsync(jobPostName, jobPostVersion, cancellationToken);
    }

    public async Task<Result<List<JobApplicationDto>>> GetCompletedJobApplicationsAsync(CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetCompletedJobApplicationsAsync(cancellationToken);
    }

    public async Task<Result<JobApplicationDto>> UpdateApplicationStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return Result<JobApplicationDto>.NotFound();

        // JobApplication doesn't have a Status property, only CompletedAt
        // For now, we'll just update CompletedAt when status is "Completed"
        if (status == "Completed")
            entity.CompletedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        var dto = _mapper.Map<JobApplicationDto>(entity);
        return Result<JobApplicationDto>.Success(dto);
    }

    // Candidate methods
    public async Task<Result<List<JobApplicationDto>>> GetMyApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
        if (candidateId == null)
            return Result<List<JobApplicationDto>>.Error();

        return await GetByCandidateIdAsync(candidateId.Value, cancellationToken);
    }

    public async Task<Result<Common.Dto.PagedResult<JobApplicationDto>>> GetMyApplicationsPagedAsync(JobApplicationListQueryDto query, CancellationToken cancellationToken = default)
    {
        var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
        if (candidateId == null)
            return Result<Common.Dto.PagedResult<JobApplicationDto>>.Unauthorized();

        return await _queryHandler.GetMyFilteredJobApplicationsAsync(candidateId.Value, query, cancellationToken);
    }

    public async Task<Result<JobApplicationDto>> ApplyForJobAsync(ApplyForJobDto applyForJobDto, CancellationToken cancellationToken = default)
    {
        // Ensure single application per candidate per job (dedupe)
        try
        {
            var result = await CreateOrGetJobApplicationAsync(applyForJobDto.JobPostName, applyForJobDto.JobPostVersion, null, cancellationToken);
            return result;
        }
        catch
        {
            return Result<JobApplicationDto>.Error();
        }
    }

    public async Task<Result<JobApplicationDto>> GetMyApplicationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
        if (candidateId == null)
            return Result<JobApplicationDto>.Error();

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return Result<JobApplicationDto>.NotFound();

        // Verify the application belongs to the candidate
        if (entity.CandidateId != candidateId.Value)
            return Result<JobApplicationDto>.Unauthorized();

        var dto = _mapper.Map<JobApplicationDto>(entity);
        return Result<JobApplicationDto>.Success(dto);
    }

    public async Task<Result<JobApplicationDto?>> GetMyApplicationByJobPostAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default)
    {
        var candidateId = await GetCurrentCandidateIdAsync(cancellationToken);
        if (candidateId == null)
            return Result<JobApplicationDto?>.Unauthorized();

        // Use optimized specification to query only the specific job application for this candidate
        var spec = new JobApplicationByJobPostAndCandidateSpec(jobPostName, jobPostVersion, candidateId.Value);
        var application = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (application == null)
            return Result<JobApplicationDto?>.Success(null); // Return null if not found, not an error

        var dto = _mapper.Map<JobApplicationDto>(application);
        return Result<JobApplicationDto?>.Success(dto);
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

    // Helper method for resume upload - creates or gets existing job application
    public async Task<Result<JobApplicationDto>> CreateOrGetJobApplicationAsync(string jobPostName, int jobPostVersion, Guid? candidateId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get or create candidate ID
            Guid actualCandidateId;
            if (candidateId.HasValue)
            {
                actualCandidateId = candidateId.Value;
            }
            else
            {
                var currentCandidateId = await GetCurrentCandidateIdAsync(cancellationToken);
                if (currentCandidateId == null)
                {
                    throw new UnauthorizedAccessException("User must be authenticated as a candidate");
                }
                actualCandidateId = currentCandidateId.Value;
            }

            // Check if application already exists
            var existingApplications = await GetByJobPostAsync(jobPostName, jobPostVersion, cancellationToken);
            if (existingApplications.IsSuccess)
            {
                var existingApp = existingApplications.Value.FirstOrDefault(app => app.CandidateId == actualCandidateId);
                if (existingApp != null)
                {
                    return Result<JobApplicationDto>.Success(existingApp);
                }
            }

            // Create new job application
            var jobApplication = new Domain.Models.JobApplication
            {
                JobPostName = jobPostName,
                JobPostVersion = jobPostVersion,
                CandidateId = actualCandidateId,
                StartedAt = DateTimeOffset.UtcNow
            };

            await _repository.AddAsync(jobApplication);
            await _repository.SaveChangesAsync();

            var dto = _mapper.Map<JobApplicationDto>(jobApplication);
            return Result<JobApplicationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create or get job application: {ex.Message}", ex);
        }
    }

    public async Task<Result<JobApplicationWithStepsAndInterviewsDto>> GetJobApplicationWithStepsAndInterviewsAsync(string jobPostName, int jobPostVersion, Guid candidateId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the job application for this candidate and job post
            var applicationsResult = await GetByJobPostAsync(jobPostName, jobPostVersion, cancellationToken);
            if (!applicationsResult.IsSuccess)
            {
                return Result<JobApplicationWithStepsAndInterviewsDto>.NotFound();
            }

            var application = applicationsResult.Value.FirstOrDefault(a => a.CandidateId == candidateId);
            if (application == null)
            {
                return Result<JobApplicationWithStepsAndInterviewsDto>.NotFound();
            }

            // Get application entity
            var applicationEntity = await _repository.GetByIdAsync(application.Id!.Value);
            if (applicationEntity == null)
            {
                return Result<JobApplicationWithStepsAndInterviewsDto>.NotFound();
            }

            // Get all steps for this application
            var steps = await _jobApplicationStepRepository.ListAsync(new JobApplicationStepByJobApplicationIdSpec(applicationEntity.Id), cancellationToken);

            var result = new JobApplicationWithStepsAndInterviewsDto
            {
                JobApplication = application,
                Steps = new List<JobApplicationStepWithInterviewsDto>()
            };

            // For each step, get interviews
            foreach (var step in steps)
            {
                var stepDto = new JobApplicationStepDto
                {
                    Id = step.Id,
                    JobApplicationId = step.JobApplicationId,
                    JobPostStepName = step.JobPostStepName,
                    JobPostStepVersion = step.JobPostStepVersion,
                    Status = step.Status.ToString(),
                    StepNumber = step.StepNumber,
                    StartedAt = step.StartedAt,
                    CompletedAt = step.CompletedAt,
                    Data = step.Data
                };

                // Get interviews for this step
                var interviews = await _interviewRepository.ListAsync(new Interview.Specifications.InterviewByJobApplicationStepIdSpec(step.Id), cancellationToken);
                var interviewDtos = _mapper.Map<List<Interview.Dto.InterviewDto>>(interviews);

                result.Steps.Add(new JobApplicationStepWithInterviewsDto
                {
                    Step = stepDto,
                    Interviews = interviewDtos
                });
            }

            return Result<JobApplicationWithStepsAndInterviewsDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<JobApplicationWithStepsAndInterviewsDto>.Invalid(new ValidationError { ErrorMessage = ex.Message });
        }
    }

    public async Task<Result<JobApplicationDto>> PromoteToNextStepAsync(Guid applicationId, Dto.PromoteStepDto promoteDto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"[DEBUG] PromoteToNextStepAsync started for application {applicationId}");
            _logger.LogInformation($"[DEBUG] PromoteDto received: JobPostName={promoteDto.JobPostName}, JobPostVersion={promoteDto.JobPostVersion}, CurrentStep={promoteDto.CurrentStep}, NextStep={promoteDto.NextStep}");
            
            // Get the job application
            var application = await _repository.GetByIdAsync(applicationId);
            if (application == null)
            {
                _logger.LogError($"[DEBUG] Application not found: {applicationId}");
                return Result<JobApplicationDto>.NotFound();
            }

            _logger.LogInformation($"[DEBUG] Application found: JobPostName={application.JobPostName}, JobPostVersion={application.JobPostVersion}");

            // Verify the application matches the job post in the request
            if (application.JobPostName != promoteDto.JobPostName || application.JobPostVersion != promoteDto.JobPostVersion)
            {
                _logger.LogError($"[DEBUG] Job post mismatch. Application: {application.JobPostName} v{application.JobPostVersion}, Request: {promoteDto.JobPostName} v{promoteDto.JobPostVersion}");
                return Result<JobApplicationDto>.Invalid(new ValidationError { ErrorMessage = "Application does not match the specified job post" });
            }

            // Get all steps for this application (ordered by step number)
            var stepsSpec = new JobApplicationStepByJobApplicationIdSpec(applicationId);
            var steps = await _jobApplicationStepRepository.ListAsync(stepsSpec, cancellationToken);
            
            // Get any existing step to use as a template
            var templateStep = steps.FirstOrDefault();
            if (templateStep == null)
            {
                _logger.LogError($"[DEBUG] No steps found for application {applicationId}");
                return Result<JobApplicationDto>.Invalid(new ValidationError { ErrorMessage = "No steps found for this application" });
            }

            _logger.LogInformation($"[DEBUG] Template step found: JobPostStepName={templateStep.JobPostStepName}, JobPostStepVersion={templateStep.JobPostStepVersion}");

            // Query the CURRENT step from JobPostStepAssignment to get the correct step name
            var spec = new JobPost.Specifications.JobPostStepAssignmentByJobPostSpec(promoteDto.JobPostName, promoteDto.JobPostVersion);
            var stepAssignments = await _jobPostStepAssignmentRepository.ListAsync(spec, cancellationToken);
            
            var currentStepAssignment = stepAssignments.FirstOrDefault(sa => sa.StepNumber == promoteDto.CurrentStep);
            
            if (currentStepAssignment == null)
            {
                _logger.LogError($"[DEBUG] Current step assignment not found in JobPostStepAssignment for StepNumber={promoteDto.CurrentStep}");
                return Result<JobApplicationDto>.Invalid(new ValidationError { ErrorMessage = "Current step not found in job post configuration" });
            }

            _logger.LogInformation($"[DEBUG] Current step assignment found: StepName={currentStepAssignment.StepName}, StepVersion={currentStepAssignment.StepVersion}");

            // CREATE ONLY ONE ROW in JobApplicationSteps with:
            // - New Id (GUID)
            // - Same JobApplicationId
            // - JobPostStepName from the CURRENT step assignment (the step being promoted FROM)
            // - JobPostStepVersion from the CURRENT step assignment
            // - StepNumber = CurrentStep (the current step being marked complete)
            // - Status = "Completed"
            // - CompletedAt = now
            var completedStepRecord = new Domain.Models.JobApplicationStep
            {
                Id = Guid.NewGuid(),
                JobApplicationId = applicationId,
                JobPostStepName = currentStepAssignment.StepName,
                JobPostStepVersion = currentStepAssignment.StepVersion ?? throw new InvalidOperationException("StepVersion cannot be null"),
                StepNumber = promoteDto.CurrentStep,
                Status = Domain.Enums.JobApplicationStepStatusEnum.Completed,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow,
                Data = null,
                CreatedBy = templateStep.CreatedBy,
                UpdatedBy = templateStep.UpdatedBy,
                IsDeleted = false
            };

            _logger.LogInformation($"[DEBUG] Creating new completed step record: StepNumber={completedStepRecord.StepNumber}, JobPostStepName={completedStepRecord.JobPostStepName}, Status=Completed");
            await _jobApplicationStepRepository.AddAsync(completedStepRecord);

            await _jobApplicationStepRepository.SaveChangesAsync();
            _logger.LogInformation($"[DEBUG] Promotion successful for application {applicationId}");

            var dto = _mapper.Map<JobApplicationDto>(application);
            return Result<JobApplicationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[DEBUG] Exception in PromoteToNextStepAsync: {ex.Message}");
            return Result<JobApplicationDto>.Invalid(new ValidationError { ErrorMessage = $"Failed to promote candidate to next step: {ex.Message}" });
        }
    }
}
