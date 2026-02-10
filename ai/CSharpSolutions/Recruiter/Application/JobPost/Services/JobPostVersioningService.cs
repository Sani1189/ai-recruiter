using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Services;

/// <summary>
/// Service for handling versioning operations for JobPost and JobPostStep entities
/// </summary>
public class JobPostVersioningService : IJobPostVersioningService
{
    private readonly IRepository<Domain.Models.JobPost> _jobPostRepository;
    private readonly IRepository<JobPostStep> _jobPostStepRepository;
    private readonly ICountryExposureSetService _countryExposureSetService;
    private readonly IMapper _mapper;

    public JobPostVersioningService(
        IRepository<Domain.Models.JobPost> jobPostRepository,
        IRepository<JobPostStep> jobPostStepRepository,
        ICountryExposureSetService countryExposureSetService,
        IMapper mapper)
    {
        _jobPostRepository = jobPostRepository;
        _jobPostStepRepository = jobPostStepRepository;
        _countryExposureSetService = countryExposureSetService;
        _mapper = mapper;
    }

    public async Task<JobPostDto> CreateJobPostVersionAsync(JobPostDto dto, int nextVersion)
    {
        // Get the existing JobPost to clone from
        var existingJobPostSpec = new JobPostByNameAndVersionSpec(dto.Name, dto.Version);
        var existingJobPost = await _jobPostRepository.FirstOrDefaultAsync(existingJobPostSpec);
        
        if (existingJobPost == null)
        {
            throw new InvalidOperationException($"JobPost with name '{dto.Name}' and version '{dto.Version}' not found.");
        }

        var newJobPost = new Domain.Models.JobPost
        {
            Name = dto.Name,
            Version = nextVersion,
            MaxAmountOfCandidatesRestriction = dto.MaxAmountOfCandidatesRestriction,
            MinimumRequirements = dto.MinimumRequirements,
            ExperienceLevel = dto.ExperienceLevel,
            JobTitle = dto.JobTitle,
            JobType = dto.JobType,
            JobDescription = dto.JobDescription,
            PoliceReportRequired = dto.PoliceReportRequired,
            Status = dto.Status,
            OriginCountryCode = NormalizeOriginCountryCode(dto.OriginCountryCode),
            CountryExposureSetId = await _countryExposureSetService.GetOrCreateSetIdAsync(dto.CountryExposureCountryCodes),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _jobPostRepository.AddAsync(newJobPost);
        await _jobPostRepository.SaveChangesAsync();

        // Do not auto-copy step assignments; the caller will assign steps explicitly based on payload

        return _mapper.Map<JobPostDto>(newJobPost);
    }

    /// <summary>Empty or whitespace OriginCountryCode would violate FK to Country; use null instead.</summary>
    private static string? NormalizeOriginCountryCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public async Task<JobPostStepDto> CreateJobPostStepVersionAsync(JobPostStepDto dto, int nextVersion)
    {
        // Get the existing JobPostStep to clone from
        var existingStepSpec = new JobPostStepByNameAndVersionSpec(dto.Name, dto.Version);
        var existingStep = await _jobPostStepRepository.FirstOrDefaultAsync(existingStepSpec);
        
        if (existingStep == null)
        {
            throw new InvalidOperationException($"JobPostStep with name '{dto.Name}' and version '{dto.Version}' not found.");
        }

        // Create new JobPostStep version with updated properties
        var newStep = new JobPostStep
        {
            Name = dto.Name, // Name cannot be changed
            Version = nextVersion,
            IsInterview = dto.Participant == "Candidate" && dto.StepType == "Interview",
            StepType = dto.StepType,
            Participant = dto.Participant,
            // Candidate steps are always visible to the candidate
            ShowStepForCandidate = dto.Participant == "Candidate" ? true : dto.ShowStepForCandidate,
            DisplayTitle = dto.DisplayTitle,
            DisplayContent = dto.DisplayContent,
            ShowSpinner = dto.Participant == "Candidate" ? false : dto.ShowSpinner,
            InterviewConfigurationName = dto.InterviewConfigurationName,
            InterviewConfigurationVersion = dto.InterviewConfigurationVersion,
            PromptName = dto.PromptName,
            PromptVersion = dto.PromptVersion,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Save the new JobPostStep version
        await _jobPostStepRepository.AddAsync(newStep);
        await _jobPostStepRepository.SaveChangesAsync();

        // Do not cascade step versioning to JobPosts automatically

        return _mapper.Map<JobPostStepDto>(newStep);
    }

    public async Task<int> GetNextJobPostVersionAsync(string name)
    {
        var spec = new JobPostByNameSpec(name);
        var existingJobPosts = await _jobPostRepository.ListAsync(spec);
        return existingJobPosts.Max(jp => jp.Version) + 1;
    }

    public async Task<int> GetNextJobPostStepVersionAsync(string name)
    {
        var spec = new JobPostStepByNameSpec(name);
        var existingSteps = await _jobPostStepRepository.ListAsync(spec);
        return existingSteps.Max(s => s.Version) + 1;
    }
}
