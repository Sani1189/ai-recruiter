using AutoMapper;
using Recruiter.Application.Common;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Application.JobPost.Queries;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost;

public class JobPostStepService : IJobPostStepService
{
    private readonly IRepository<JobPostStep> _repository;
    private readonly IRepository<JobPostStepAssignment> _jobPostStepAssignmentRepository;
    private readonly IRepository<JobApplicationStep> _jobApplicationStepRepository;
    private readonly IMapper _mapper;
    private readonly JobPostQueryHandler _queryHandler;
    private readonly IJobPostVersioningService _versioningService;

    public JobPostStepService(
        IRepository<JobPostStep> repository, 
        IRepository<JobPostStepAssignment> jobPostStepAssignmentRepository,
        IRepository<JobApplicationStep> jobApplicationStepRepository,
        IMapper mapper, 
        JobPostQueryHandler queryHandler, 
        IJobPostVersioningService versioningService)
    {
        _repository = repository;
        _jobPostStepAssignmentRepository = jobPostStepAssignmentRepository;
        _jobApplicationStepRepository = jobApplicationStepRepository;
        _mapper = mapper;
        _queryHandler = queryHandler;
        _versioningService = versioningService;
    }

    public async Task<JobPostStepDto?> GetByIdAsync(string name, int version)
    {
        var result = await _queryHandler.GetStepByIdAsync(name, version);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<JobPostStepDto?> GetLatestVersionAsync(string name)
    {
        var spec = new JobPostStepLatestByNameSpec(name);
        var latest = await _repository.FirstOrDefaultAsync(spec);
        return latest != null ? _mapper.Map<JobPostStepDto>(latest) : null;
    }


    public async Task<IEnumerable<JobStepVersionDto>> GetAllVersionsAsync(string name)
    {
        var spec = new JobPostStepByNameSpec(name);
        var results = await _repository.ListAsync(spec);
         return _mapper.Map<IEnumerable<JobStepVersionDto>>(results);
    }

    public async Task<PagedResult<JobPostStepDto>> GetListAsync(JobPostStepQueryDto query)
    {
        var result = await _queryHandler.GetStepListAsync(query);
        return result.IsSuccess ? result.Value : new PagedResult<JobPostStepDto>();
    }

    public async Task<IEnumerable<JobPostStepDto>> GetDropdownListAsync()
    {
        var result = await _queryHandler.GetStepDropdownListAsync();
        return result.IsSuccess ? result.Value : Enumerable.Empty<JobPostStepDto>();
    }

    public async Task<JobPostStepDto> CreateAsync(JobPostStepDto dto)
    {
        // Back-compat: normalize legacy value
        if (string.Equals(dto.StepType, QuestionnaireConstants.StepTypes.MultipleChoiceLegacy, StringComparison.OrdinalIgnoreCase))
        {
            dto.StepType = QuestionnaireConstants.StepTypes.Questionnaire;
        }
        if (string.Equals(dto.StepType, QuestionnaireConstants.StepTypes.AssessmentLegacy, StringComparison.OrdinalIgnoreCase))
        {
            dto.StepType = QuestionnaireConstants.StepTypes.Questionnaire;
        }

        var isInterview = dto.Participant == "Candidate" && dto.StepType == "Interview";
        var isAssessment = dto.Participant == "Candidate" && dto.StepType == QuestionnaireConstants.StepTypes.Questionnaire;

        // Candidate steps are always visible to the candidate (ShowStepForCandidate is primarily for recruiter steps)
        if (dto.Participant == "Candidate")
        {
            dto.ShowStepForCandidate = true;
        }

        // Only interview steps can have interview configuration
        if (!isInterview)
        {
            dto.InterviewConfigurationName = null;
            dto.InterviewConfigurationVersion = null;
        }

        // Only assessment steps can have assessment template
        if (!isAssessment)
        {
            dto.QuestionnaireTemplateName = null;
            dto.QuestionnaireTemplateVersion = null;
        }

        // Candidate steps don't support spinner configuration
        if (dto.Participant == "Candidate")
        {
            dto.ShowSpinner = false;
        }

        if (!dto.ShowStepForCandidate)
        {
            dto.DisplayTitle = null;
            dto.DisplayContent = null;
            dto.ShowSpinner = false;
        }

        // Check if a step with the same name already exists
        var existingStep = await GetLatestVersionAsync(dto.Name);
        
        if (existingStep != null)
        {
            // Step already exists, check if it's a duplicate
            if (AreJobPostStepsEqual(existingStep, dto))
            {
                // It's a duplicate, throw an exception
                throw new InvalidOperationException($"A JobPostStep with name '{dto.Name}' and identical content already exists. Use UpdateAsync to create a new version.");
            }
        }
        
         // New step, create version 1
        dto.Version = 1;
        var newEntity = _mapper.Map<Domain.Models.JobPostStep>(dto);
        newEntity.IsInterview = isInterview;
        await _repository.AddAsync(newEntity);
        await _repository.SaveChangesAsync();
        return _mapper.Map<JobPostStepDto>(newEntity);
    }

    public async Task<JobPostStepDto> UpdateAsync(JobPostStepDto dto)
    {
        bool shouldUpdateVersion = dto.ShouldUpdateVersion ?? false;

        if (!shouldUpdateVersion)
        {
            var inUse = await IsJobPostStepInUseAsync(dto.Name, dto.Version);
            if (inUse)
            {
                throw new InvalidOperationException(
                    "This step has existing applications and cannot be updated. Create a new version to make changes.");
            }
        }

        if (shouldUpdateVersion)
        {
            var nextVersion = await _versioningService.GetNextJobPostStepVersionAsync(dto.Name);
            return await _versioningService.CreateJobPostStepVersionAsync(dto, nextVersion);
        }

        return await UpdateExistingVersionAsync(dto);
    }

    public async Task<JobPostStepDto?> DuplicateAsync(string sourceName, int sourceVersion, DuplicateJobPostStepRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.NewName)) throw new InvalidOperationException("New step name is required.");

        // Load source step
        var source = await _repository.FirstOrDefaultAsync(new JobPostStepByNameAndVersionSpec(sourceName, sourceVersion));
        if (source == null)
        {
            return null;
        }

        // Reuse CreateAsync invariants (candidate visibility/spinner + interview config rules)
        var dto = _mapper.Map<JobPostStepDto>(source);
        dto.Name = request.NewName.Trim();
        dto.DisplayTitle = request.NewDisplayTitle ?? dto.DisplayTitle;
        dto.Version = 1;
        dto.ShouldUpdateVersion = false;

        return await CreateAsync(dto);
    }

    public async Task DeleteAsync(string name, int version)
    {
        var spec = new JobPostStepByNameAndVersionSpec(name, version);
        var entity = await _repository.FirstOrDefaultAsync(spec);
        if (entity == null)
            return;

        // Check if job post step is in use
        bool isInUse = await IsJobPostStepInUseAsync(name, version);

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

    public async Task<bool> IsJobPostStepInUseAsync(string name, int version)
    {
        // Check if used in JobPostStepAssignment (explicit version)
        var explicitAssignmentsCount = await _jobPostStepAssignmentRepository.CountAsync(
            new JobPostStepAssignmentsByStepNameAndVersionSpec(name, version));

        if (explicitAssignmentsCount > 0)
            return true;

        // Check dynamic assignments (StepVersion == null means "use latest") only blocks deleting the latest version
        var latest = await GetLatestVersionAsync(name);
        if (latest != null && latest.Version == version)
        {
            var dynamicAssignmentsCount = await _jobPostStepAssignmentRepository.CountAsync(
                new JobPostStepAssignmentsByStepNameWithDynamicLatestSpec(name));

            if (dynamicAssignmentsCount > 0)
                return true;
        }

        // Check if used in JobApplicationStep
        var applicationStepsCount = await _jobApplicationStepRepository.CountAsync(
            new JobApplicationStepsByJobPostStepSpec(name, version));

        return applicationStepsCount > 0;
    }

    public async Task RestoreJobPostStepAsync(string name, int version)
    {
        var spec = new JobPostStepByNameAndVersionSpec(name, version);
        var entity = await _repository.FirstOrDefaultAsync(spec);
        
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
        var spec = new JobPostStepByNameAndVersionSpec(name, version);
        var entity = await _repository.FirstOrDefaultAsync(spec);
        return entity != null;
    }

    private async Task<JobPostStepDto> UpdateExistingVersionAsync(JobPostStepDto dto)
    {
        // Back-compat: normalize legacy value
        if (string.Equals(dto.StepType, QuestionnaireConstants.StepTypes.MultipleChoiceLegacy, StringComparison.OrdinalIgnoreCase))
        {
            dto.StepType = QuestionnaireConstants.StepTypes.Questionnaire;
        }
        if (string.Equals(dto.StepType, QuestionnaireConstants.StepTypes.AssessmentLegacy, StringComparison.OrdinalIgnoreCase))
        {
            dto.StepType = QuestionnaireConstants.StepTypes.Questionnaire;
        }
        // Get the existing entity from database (this will be tracked by EF)
        var spec = new JobPostStepByNameAndVersionSpec(dto.Name, dto.Version);
        var existingEntity = await _repository.FirstOrDefaultAsync(spec);
        
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"JobPostStep with name '{dto.Name}' and version '{dto.Version}' not found.");
        }

        // Update the existing entity's properties (don't create new entity)
        var isInterview = dto.Participant == "Candidate" && dto.StepType == "Interview";
        var isAssessment = dto.Participant == "Candidate" && dto.StepType == QuestionnaireConstants.StepTypes.Questionnaire;
        existingEntity.IsInterview = isInterview;
        existingEntity.StepType = dto.StepType;
        existingEntity.Participant = dto.Participant;
        // Candidate steps are always visible to the candidate
        existingEntity.ShowStepForCandidate = dto.Participant == "Candidate" ? true : dto.ShowStepForCandidate;
        existingEntity.DisplayTitle = dto.DisplayTitle;
        existingEntity.DisplayContent = dto.DisplayContent;
        existingEntity.ShowSpinner = dto.Participant == "Candidate" ? false : dto.ShowSpinner;

        if (isInterview)
        {
            existingEntity.InterviewConfigurationName = dto.InterviewConfigurationName;
            existingEntity.InterviewConfigurationVersion = dto.InterviewConfigurationVersion;
        }
        else
        {
            existingEntity.InterviewConfigurationName = null;
            existingEntity.InterviewConfigurationVersion = null;
        }

        if (isAssessment)
        {
            existingEntity.QuestionnaireTemplateName = dto.QuestionnaireTemplateName;
            existingEntity.QuestionnaireTemplateVersion = dto.QuestionnaireTemplateVersion;
        }
        else
        {
            existingEntity.QuestionnaireTemplateName = null;
            existingEntity.QuestionnaireTemplateVersion = null;
        }
        existingEntity.PromptName = dto.PromptName;
        existingEntity.PromptVersion = dto.PromptVersion;
        existingEntity.UpdatedAt = DateTimeOffset.UtcNow;

        // Update the tracked entity
        await _repository.UpdateAsync(existingEntity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<JobPostStepDto>(existingEntity);
    }

    private static bool AreJobPostStepsEqual(JobPostStepDto existing, JobPostStepDto incoming)
    {
        var existingIsInterview = existing.Participant == "Candidate" && existing.StepType == "Interview";
        var incomingIsInterview = incoming.Participant == "Candidate" && incoming.StepType == "Interview";

        return existingIsInterview == incomingIsInterview &&
               existing.StepType == incoming.StepType &&
               existing.Participant == incoming.Participant &&
               existing.ShowStepForCandidate == incoming.ShowStepForCandidate &&
               existing.DisplayTitle == incoming.DisplayTitle &&
               existing.DisplayContent == incoming.DisplayContent &&
               existing.ShowSpinner == incoming.ShowSpinner &&
               existing.InterviewConfigurationName == incoming.InterviewConfigurationName &&
               existing.InterviewConfigurationVersion == incoming.InterviewConfigurationVersion &&
               existing.PromptName == incoming.PromptName &&
               existing.PromptVersion == incoming.PromptVersion &&
               existing.QuestionnaireTemplateName == incoming.QuestionnaireTemplateName &&
               existing.QuestionnaireTemplateVersion == incoming.QuestionnaireTemplateVersion;
    }
}