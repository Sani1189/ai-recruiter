using AutoMapper;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobApplication.Specifications;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost;

public class JobPostOrchestrator : IJobPostOrchestrator
{
    private readonly IJobPostService _jobPostService;
    private readonly IJobPostStepService _jobPostStepService;
    private readonly IJobPostStepAssignmentService _stepAssignmentService;
    private readonly IRepository<Domain.Models.JobPost> _repository;
    private readonly IRepository<JobPostStepAssignment> _jobPostStepAssignmentRepository;
    private readonly IRepository<Domain.Models.JobApplication> _jobApplicationRepository;
    private readonly IMapper _mapper;

    public JobPostOrchestrator(
        IJobPostService jobPostService,
        IJobPostStepService jobPostStepService,
        IJobPostStepAssignmentService stepAssignmentService,
        IRepository<Domain.Models.JobPost> repository,
        IRepository<JobPostStepAssignment> jobPostStepAssignmentRepository,
        IRepository<Domain.Models.JobApplication> jobApplicationRepository,
        IMapper mapper
    )
    {
        _jobPostService = jobPostService;
        _jobPostStepService = jobPostStepService;
        _stepAssignmentService = stepAssignmentService;
        _repository = repository;
        _jobPostStepAssignmentRepository = jobPostStepAssignmentRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _mapper = mapper;
    }

    public async Task<JobPostDto> CreateJobPostWithStepsAsync(JobPostDto jobPostDto)
    {        
        // Check if a JobPost with the same name already exists
        var existingJobPost = await _jobPostService.GetLatestVersionAsync(jobPostDto.Name);
        if (existingJobPost != null)
        {
            throw new InvalidOperationException($"A JobPost with name '{jobPostDto.Name}' already exists. Use UpdateAsync to create a new version.");
        }
        
        // Create the job post first (service will handle version and validation)
        var createdJobPost = await _jobPostService.CreateAsync(jobPostDto);

        // Handle step assignments if provided
        if (jobPostDto.Steps != null && jobPostDto.Steps.Any())
        {
            foreach (var stepRequest in jobPostDto.Steps)
            {
                if (stepRequest.IsValid())
                {
                    JobPostStepDto stepDto;
                    
                    if (!string.IsNullOrEmpty(stepRequest.ExistingStepName))
                    {
                        // Use existing step 
                        var existingStep =
                        stepRequest.ExistingStepVersion.HasValue ?
                            await _jobPostStepService.GetByIdAsync(
                            stepRequest.ExistingStepName,
                            stepRequest.ExistingStepVersion!.Value)
                            : await _jobPostStepService.GetLatestVersionAsync(stepRequest.ExistingStepName);
                        
                        
                        if (existingStep != null)
                        {
                            stepDto = existingStep;
                        }
                        else
                        {
                            continue; // Skip invalid step reference
                        }
                    }
                    else if (stepRequest.NewStep != null)
                    {
                        // Create new step
                        stepDto = await _jobPostStepService.CreateAsync(stepRequest.NewStep);
                    }
                    else
                    {
                        continue; // Skip invalid step request
                    }

                    // Assign step to job post
                    var assignment = new JobPostStepAssignmentDto
                    {
                        JobPostName = createdJobPost.Name,
                        JobPostVersion = createdJobPost.Version,
                        StepNumber = stepRequest.StepNumber,
                        Status = "pending",
                        StepName = stepDto.Name,
                        // If ExistingStepVersion was provided, use it; otherwise null = use latest dynamically
                        StepVersion = stepRequest.ExistingStepVersion,
                        StepDetails = stepDto
                    };

                    try
                    {
                        await _stepAssignmentService.AssignStepAsync(assignment);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with other steps
                        Console.WriteLine($"Error assigning step {stepDto.Name}: {ex.Message}");
                    }
                }
            }
        }

        // Return job post with assigned steps
        return await GetJobPostWithStepsAsync(createdJobPost.Name, createdJobPost.Version) ?? createdJobPost;
    }

    public async Task<JobPostDto?> GetJobPostWithStepsAsync(string name, int version)
    {
        var jobPost = await _jobPostService.GetByIdAsync(name, version);
        if (jobPost == null)
        {
            return null;
        }

        // Get assigned steps
        var assignedSteps = await _stepAssignmentService.GetByJobPostAsync(name, version);
        jobPost.AssignedSteps = assignedSteps.ToList();

        return jobPost;
    }

    public async Task<JobPostDto?> GetPublishedJobPostWithStepsAsync(string name, int version)
    {
        var jobPost = await _jobPostService.GetPublishedByIdAsync(name, version);
        if (jobPost == null)
            return null;

        var assignedSteps = await _stepAssignmentService.GetByJobPostAsync(name, version);
        jobPost.AssignedSteps = assignedSteps.ToList();
        return jobPost;
    }

    public async Task<JobPostDto?> GetLatestPublishedJobPostWithStepsAsync(string name)
    {
        var jobPost = await _jobPostService.GetLatestPublishedVersionAsync(name);
        if (jobPost == null)
            return null;

        var assignedSteps = await _stepAssignmentService.GetByJobPostAsync(jobPost.Name, jobPost.Version);
        jobPost.AssignedSteps = assignedSteps.ToList();
        return jobPost;
    }

    public async Task<IEnumerable<JobPostDto>> GetJobPostsWithStepsAsync(IEnumerable<JobPostDto> jobPosts)
    {
        // Get job posts with their assigned steps
        var jobPostsWithSteps = new List<JobPostDto>();
        
        foreach (var jobPost in jobPosts)
        {
            var jobPostWithSteps = await GetJobPostWithStepsAsync(jobPost.Name, jobPost.Version);
            if (jobPostWithSteps != null)
            {
                jobPostsWithSteps.Add(jobPostWithSteps);
            }
        }
        
        return jobPostsWithSteps;
    }

    public async Task<PagedResult<JobPostDto>> GetJobPostsWithStepsPagedAsync(JobPostListQueryDto query)
    {
        // Get paginated job posts from service
        var pagedJobPosts = await _jobPostService.GetListAsync(query);
        
        // Get job posts with their assigned steps
        var jobPostsWithSteps = new List<JobPostDto>();
        
        foreach (var jobPost in pagedJobPosts.Items)
        {
            var jobPostWithSteps = await GetJobPostWithStepsAsync(jobPost.Name, jobPost.Version);
            if (jobPostWithSteps != null)
            {
                jobPostsWithSteps.Add(jobPostWithSteps);
            }
        }
        
        // Return paginated result with steps
        return new PagedResult<JobPostDto>
        {
            Items = jobPostsWithSteps,
            TotalCount = pagedJobPosts.TotalCount,
            PageNumber = pagedJobPosts.PageNumber,
            PageSize = pagedJobPosts.PageSize
        };
    }

    public async Task<JobPostDto> UpdateJobPostWithStepsAsync(JobPostDto jobPostDto)
    {
        // If not creating a new version, block update when job post has any applications
        if (!(jobPostDto.ShouldUpdateVersion ?? false))
        {
            var applicationsCount = await _jobApplicationRepository.CountAsync(
                new JobApplicationByJobPostSpec(jobPostDto.Name, jobPostDto.Version));
            if (applicationsCount > 0)
            {
                throw new InvalidOperationException(
                    "This job post has existing applications. Create a new version to make changes.");
            }
        }

        var updatedJobPost = await _jobPostService.UpdateAsync(jobPostDto);

        // Synchronize step assignments exactly with payload:
        // - Remove assignments that are not in payload
        // - Add only new assignments
        if (jobPostDto.Steps != null)
        {
            // Fetch current assignments for this job post
            var existingAssignments = (await _stepAssignmentService.GetByJobPostAsync(updatedJobPost.Name, updatedJobPost.Version)).ToList();

            // Build desired set from payload
            var desired = jobPostDto.Steps
                .Where(sr => sr.IsValid())
                .Select(sr => new {
                    sr.StepNumber,
                    StepName = sr.ExistingStepName ?? sr.NewStep?.Name,
                    StepVersion = sr.ExistingStepVersion
                })
                .Where(x => !string.IsNullOrEmpty(x.StepName))
                .ToList();

            // Remove assignments not present in desired
            foreach (var assignment in existingAssignments)
            {
                var stillPresent = desired.Any(d => d.StepName == assignment.StepName && d.StepVersion == assignment.StepVersion);
                if (!stillPresent && assignment.StepName != null)
                {
                    await _stepAssignmentService.UnassignStepAsync(updatedJobPost.Name, updatedJobPost.Version, assignment.StepName, assignment.StepVersion);
                }
            }

            // Adjust step numbers: for assignments that remain but with a different StepNumber
            foreach (var assignment in existingAssignments)
            {
                if (assignment.StepName == null) continue;
                var target = desired.FirstOrDefault(d => d.StepName == assignment.StepName && d.StepVersion == assignment.StepVersion);
                if (target != null && target.StepNumber != assignment.StepNumber)
                {
                    // Recreate assignment with new step number
                    await _stepAssignmentService.UnassignStepAsync(updatedJobPost.Name, updatedJobPost.Version, assignment.StepName, assignment.StepVersion);
                    var reassignment = new JobPostStepAssignmentDto
                    {
                        JobPostName = updatedJobPost.Name,
                        JobPostVersion = updatedJobPost.Version,
                        StepNumber = target.StepNumber,
                        Status = assignment.Status,
                        StepName = assignment.StepName,
                        StepVersion = assignment.StepVersion,
                    };
                    try
                    {
                        await _stepAssignmentService.AssignStepAsync(reassignment);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reassigning step {assignment.StepName} with new order: {ex.Message}");
                    }
                }
            }

            // Add new assignments present in desired but not existing
            foreach (var stepRequest in jobPostDto.Steps)
            {
                if (stepRequest.IsValid())
                {
                    JobPostStepDto stepDto;
                    
                    if (!string.IsNullOrEmpty(stepRequest.ExistingStepName))
                    {
                        // Use existing step - either specific version or latest
                        var existingStep =
                        stepRequest.ExistingStepVersion.HasValue ?
                            await _jobPostStepService.GetByIdAsync(
                            stepRequest.ExistingStepName,
                            stepRequest.ExistingStepVersion.Value)
                            : await _jobPostStepService.GetLatestVersionAsync(stepRequest.ExistingStepName);
                        
                        if (existingStep != null)
                        {
                            stepDto = existingStep;
                        }
                        else
                        {
                            continue; // Skip invalid step reference
                        }
                    }
                    else if (stepRequest.NewStep != null)
                    {
                        // Create new step
                        stepDto = await _jobPostStepService.CreateAsync(stepRequest.NewStep);
                    }
                    else
                    {
                        continue; // Skip invalid step request
                    }

                    // Add only if not already assigned
                    var exists = existingAssignments.Any(a => a.StepName == stepDto.Name && a.StepVersion == stepRequest.ExistingStepVersion);
                    if (!exists)
                    {
                        var assignment = new JobPostStepAssignmentDto
                        {
                            JobPostName = updatedJobPost.Name,
                            JobPostVersion = updatedJobPost.Version,
                            StepNumber = stepRequest.StepNumber,
                            Status = "pending",
                            StepName = stepDto.Name,
                            // If ExistingStepVersion was provided, use it; otherwise null = use latest dynamically
                            StepVersion = stepRequest.ExistingStepVersion,
                            StepDetails = stepDto
                        };

                        try
                        {
                            await _stepAssignmentService.AssignStepAsync(assignment);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error assigning step {stepDto.Name}: {ex.Message}");
                        }
                    }
                }
            }
        }

        // Return job post with assigned steps
        return await GetJobPostWithStepsAsync(updatedJobPost.Name, updatedJobPost.Version) ?? updatedJobPost;
    }

    public async Task<JobPostDto?> DuplicateJobPostWithStepsAsync(string sourceName, int sourceVersion, DuplicateJobPostRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.NewName)) throw new InvalidOperationException("New job post name is required.");
        if (string.IsNullOrWhiteSpace(request.NewJobTitle)) throw new InvalidOperationException("New job title is required.");

        var sourceSpec = new JobPostByNameAndVersionWithCountryExposuresSpec(sourceName, sourceVersion);
        var source = await _repository.FirstOrDefaultAsync(sourceSpec);
        if (source == null)
        {
            return null;
        }

        // Ensure the target name is not already in use (any version)
        var existingTarget = await _repository.FirstOrDefaultAsync(new JobPostLatestByNameSpec(request.NewName));
        if (existingTarget != null)
        {
            throw new InvalidOperationException($"A JobPost with name '{request.NewName}' already exists.");
        }

        // Load source step assignments (ordered)
        var sourceAssignments = await _jobPostStepAssignmentRepository.ListAsync(
            new JobPostStepAssignmentByJobPostSpec(sourceName, sourceVersion));

        var now = DateTimeOffset.UtcNow;

        var duplicated = new Domain.Models.JobPost
        {
            Name = request.NewName,
            Version = 1,
            JobTitle = request.NewJobTitle,
            JobType = source.JobType,
            ExperienceLevel = source.ExperienceLevel,
            JobDescription = source.JobDescription,
            PoliceReportRequired = source.PoliceReportRequired,
            MaxAmountOfCandidatesRestriction = source.MaxAmountOfCandidatesRestriction,
            MinimumRequirements = source.MinimumRequirements?.ToList() ?? new List<string>(),
            Status = source.Status,
            OriginCountryCode = NormalizeOriginCountryCode(source.OriginCountryCode),
            CountryExposureSetId = source.CountryExposureSetId,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddAsync(duplicated);

        foreach (var assignment in sourceAssignments)
        {
            var newAssignment = new JobPostStepAssignment
            {
                Id = Guid.NewGuid(),
                JobPostName = duplicated.Name,
                JobPostVersion = duplicated.Version,
                StepName = assignment.StepName,
                StepVersion = assignment.StepVersion,
                StepNumber = assignment.StepNumber,
                Status = "pending",
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _jobPostStepAssignmentRepository.AddAsync(newAssignment);
        }

        await _repository.SaveChangesAsync();

        return await GetJobPostWithStepsAsync(duplicated.Name, duplicated.Version) ?? _mapper.Map<JobPostDto>(duplicated);
    }

    public async Task<Ardalis.Result.Result> DeleteJobPostWithStepsAsync(string name, int version)
    {
        var spec = new JobPostByNameAndVersionSpec(name, version);
        var entity = await _repository.FirstOrDefaultAsync(spec);
        if (entity == null)
            return Ardalis.Result.Result.NotFound();

        // Check if there are job applications linked to this job post
        var linkedApplicationsSpec = new JobApplicationByJobPostSpec(name, version);
        var linkedApplicationsCount = await _jobApplicationRepository.CountAsync(linkedApplicationsSpec);

        if (linkedApplicationsCount > 0)
        {
            // Soft delete - mark as deleted
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _repository.UpdateAsync(entity);
        }
        else
        {
            // Hard delete - EF cascade will remove JobPostStepAssignments for this JobPost
            await _repository.DeleteAsync(entity);
        }

        await _repository.SaveChangesAsync();
        return Ardalis.Result.Result.Success();
    }

    /// <summary>Empty or whitespace OriginCountryCode would violate FK to Country; use null instead.</summary>
    private static string? NormalizeOriginCountryCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}