using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost;

public class JobPostStepAssignmentService : IJobPostStepAssignmentService
{
    private readonly IRepository<JobPostStepAssignment> _repository;
    private readonly IRepository<Domain.Models.JobPost> _jobPostRepository;
    private readonly IRepository<JobPostStep> _jobPostStepRepository;
    private readonly IMapper _mapper;

    public JobPostStepAssignmentService(
        IRepository<JobPostStepAssignment> repository, 
        IRepository<Domain.Models.JobPost> jobPostRepository,
        IRepository<JobPostStep> jobPostStepRepository,
        IMapper mapper)
    {
        _repository = repository;
        _jobPostRepository = jobPostRepository;
        _jobPostStepRepository = jobPostStepRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<JobPostStepAssignmentDto>> GetByJobPostAsync(string jobPostName, int jobPostVersion)
    {
        var spec = new JobPostStepAssignmentByJobPostSpec(jobPostName, jobPostVersion);
        var entities = await _repository.ListAsync(spec);
        var dtos = _mapper.Map<List<JobPostStepAssignmentDto>>(entities);

        if (!dtos.Any()) return dtos;

        // Batch fetch all required steps in a single query (eliminates N+1 problem)
        var stepNames = dtos.Select(d => d.StepName).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
        if (!stepNames.Any()) return dtos;

        var stepsSpec = new JobPostStepsByNamesSpec(stepNames!);
        var allSteps = await _jobPostStepRepository.ListAsync(stepsSpec);

        // Create lookups for fast access
        var latestStepsLookup = allSteps
            .GroupBy(s => s.Name)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.Version).First());

        var specificStepsLookup = allSteps
            .ToDictionary(s => (s.Name, s.Version));

        // Populate step details using in-memory lookups
        foreach (var dto in dtos)
        {
            if (string.IsNullOrEmpty(dto.StepName)) continue;

            JobPostStep? step = null;

            if (!dto.StepVersion.HasValue)
            {
                // Use latest version
                latestStepsLookup.TryGetValue(dto.StepName, out step);
            }
            else
            {
                // Use specific version
                specificStepsLookup.TryGetValue((dto.StepName, dto.StepVersion.Value), out step);
            }

            if (step != null)
            {
                dto.StepDetails = _mapper.Map<JobPostStepDto>(step);
            }
        }

        return dtos;
    }

    public async Task<JobPostStepAssignmentDto> AssignStepAsync(JobPostStepAssignmentDto dto)
    {
        // Validate that the JobPost exists
        var jobPostSpec = new JobPostByNameAndVersionSpec(dto.JobPostName, dto.JobPostVersion);
        var jobPost = await _jobPostRepository.FirstOrDefaultAsync(jobPostSpec);
        if (jobPost == null)
        {
            throw new InvalidOperationException($"JobPost '{dto.JobPostName}' version {dto.JobPostVersion} not found.");
        }

        // Determine step name and version for validation
        string stepName;
        int? stepVersion;

        if (!string.IsNullOrEmpty(dto.StepName))
        {
            // Use StepName and StepVersion from DTO (null version = use latest dynamically)
            stepName = dto.StepName;
            stepVersion = dto.StepVersion;
        }
        else if (dto.StepDetails != null)
        {
            // Fallback to StepDetails for backward compatibility
            stepName = dto.StepDetails.Name;
            stepVersion = dto.StepDetails.Version;
        }
        else
        {
            throw new InvalidOperationException("Either StepName or StepDetails is required for step assignment.");
        }

        // Validate that the JobPostStep exists (check specific version if provided, or just check name exists)
        if (stepVersion.HasValue)
        {
            var stepSpec = new JobPostStepByNameAndVersionSpec(stepName, stepVersion.Value);
            var step = await _jobPostStepRepository.FirstOrDefaultAsync(stepSpec);
            if (step == null)
            {
                throw new InvalidOperationException($"JobPostStep '{stepName}' version {stepVersion.Value} not found.");
            }
        }
        else
        {
            // When version is null (use latest), just validate that step name exists
            var stepSpec = new JobPostStepByNameSpec(stepName);
            var step = await _jobPostStepRepository.FirstOrDefaultAsync(stepSpec);
            if (step == null)
            {
                throw new InvalidOperationException($"JobPostStep '{stepName}' not found.");
            }
        }

        // Check if assignment already exists
        var existingAssignmentSpec = new JobPostStepAssignmentByAssignmentSpec(dto.JobPostName, dto.JobPostVersion, stepName, stepVersion);
        var existingAssignment = await _repository.FirstOrDefaultAsync(existingAssignmentSpec);
        if (existingAssignment != null)
        {
            var versionText = stepVersion.HasValue ? $"version {stepVersion.Value}" : "latest version";
            throw new InvalidOperationException($"Step assignment already exists for JobPost '{dto.JobPostName}' version {dto.JobPostVersion} and Step '{stepName}' {versionText}.");
        }

        var entity = _mapper.Map<JobPostStepAssignment>(dto);
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        
        var resultDto = _mapper.Map<JobPostStepAssignmentDto>(entity);
        
        // Populate StepDetails based on whether a specific version was assigned
        if (resultDto.StepVersion.HasValue)
        {
            var stepSpec = new JobPostStepByNameAndVersionSpec(resultDto.StepName!, resultDto.StepVersion.Value);
            var step = await _jobPostStepRepository.FirstOrDefaultAsync(stepSpec);
            if (step != null)
            {
                resultDto.StepDetails = _mapper.Map<JobPostStepDto>(step);
            }
        }
        else
        {
            // Fetch latest version
            var latestStepSpec = new JobPostStepLatestByNameSpec(resultDto.StepName!);
            var latestStep = await _jobPostStepRepository.FirstOrDefaultAsync(latestStepSpec);
            if (latestStep != null)
            {
                resultDto.StepDetails = _mapper.Map<JobPostStepDto>(latestStep);
            }
        }
        
        return resultDto;
    }

    public async Task UnassignStepAsync(string jobPostName, int jobPostVersion, string stepName, int? stepVersion)
    {
        var spec = new JobPostStepAssignmentByAssignmentSpec(jobPostName, jobPostVersion, stepName, stepVersion);
        var assignment = await _repository.FirstOrDefaultAsync(spec);
        
        if (assignment != null)
        {
            await _repository.DeleteAsync(assignment);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task UpdateAssignmentStatusAsync(string jobPostName, int jobPostVersion, string stepName, int? stepVersion, string status)
    {
        var spec = new JobPostStepAssignmentByAssignmentSpec(jobPostName, jobPostVersion, stepName, stepVersion);
        var assignment = await _repository.FirstOrDefaultAsync(spec);
        
        if (assignment != null)
        {
            assignment.Status = status;
            await _repository.UpdateAsync(assignment);
            await _repository.SaveChangesAsync();
        }
    }
}