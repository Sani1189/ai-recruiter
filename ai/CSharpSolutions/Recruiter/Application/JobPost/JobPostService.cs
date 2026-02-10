using AutoMapper;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Exceptions;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Application.JobPost.Queries;
using Recruiter.Application.JobPost.Specifications;

namespace Recruiter.Application.JobPost;

public class JobPostService : IJobPostService
{
    private readonly IRepository<Domain.Models.JobPost> _repository;
    private readonly IMapper _mapper;
    private readonly JobPostQueryHandler _queryHandler;
    private readonly IJobPostVersioningService _versioningService;
    private readonly IJobPostCandidateService _candidateService;
    private readonly IJobPostStepAssignmentService _stepAssignmentService;
    private readonly ICountryExposureSetService _countryExposureSetService;

    public JobPostService(
        IRepository<Domain.Models.JobPost> repository,
        IMapper mapper,
        JobPostQueryHandler queryHandler,
        IJobPostVersioningService versioningService,
        IJobPostCandidateService candidateService,
        IJobPostStepAssignmentService stepAssignmentService,
        ICountryExposureSetService countryExposureSetService)
    {
        _repository = repository;
        _mapper = mapper;
        _queryHandler = queryHandler;
        _versioningService = versioningService;
        _candidateService = candidateService;
        _stepAssignmentService = stepAssignmentService;
        _countryExposureSetService = countryExposureSetService;
    }

    public async Task<JobPostDto?> GetByIdAsync(string name, int version)
    {
        var result = await _queryHandler.GetByIdAsync(name, version);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<JobPostDto?> GetLatestVersionAsync(string name)
    {
        var spec = new JobPostLatestByNameSpec(name);
        var latest = await _repository.FirstOrDefaultAsync(spec);
        return latest != null ? _mapper.Map<JobPostDto>(latest) : null;
    }

    /// <summary>Get published job by name and version for public/candidate. Throws when job exists but is not published.</summary>
    public async Task<JobPostDto?> GetPublishedByIdAsync(string name, int version)
    {
        var result = await _queryHandler.GetPublishedByIdAsync(name, version);
        if (result.IsSuccess)
            return result.Value;
        if (result.Status == Ardalis.Result.ResultStatus.NotFound && await ExistsAsync(name, version))
            throw new JobPostNotAvailableException();
        return null;
    }

    /// <summary>Get latest published version by name for public/candidate. Throws when job exists but has no published version.</summary>
    public async Task<JobPostDto?> GetLatestPublishedVersionAsync(string name)
    {
        var result = await _queryHandler.GetLatestPublishedByNameAsync(name);
        if (result.IsSuccess)
            return result.Value;
        if (result.Status == Ardalis.Result.ResultStatus.NotFound && await GetLatestVersionAsync(name) != null)
            throw new JobPostNotAvailableException();
        return null;
    }

    public async Task<IEnumerable<JobPostDto>> GetEditHistoryAsync(string name)
    {
        var result = await _queryHandler.GetEditHistoryAsync(name);
        return result.IsSuccess ? result.Value : Enumerable.Empty<JobPostDto>();
    }

    public async Task<IEnumerable<JobPostDto>> GetAllVersionsAsync(string name)
    {
        var spec = new JobPostByNameSpec(name);
        var results = await _repository.ListAsync(spec);
        return results.Select(_mapper.Map<JobPostDto>);
    }

    public async Task<PagedResult<JobPostDto>> GetListAsync(JobPostListQueryDto query)
    {
        var result = await _queryHandler.GetFilteredAsync(query);
        return result.IsSuccess ? result.Value : new PagedResult<JobPostDto>();
    }

    public async Task<JobPostDto> CreateAsync(JobPostDto dto)
    {
        // Check if a job post with the same name already exists
        var existingJobPost = await GetLatestVersionAsync(dto.Name);
        
        if (existingJobPost != null)
        {
            // Job post already exists, check if it's a duplicate
            if (AreJobPostsEqual(existingJobPost, dto))
            {
                // It's a duplicate, throw an exception
                throw new InvalidOperationException($"A JobPost with name '{dto.Name}' and identical content already exists. Use Update to create a new version.");
            }
        }
        // New job post, set version to 1
        dto.Version = 1;

        var newEntity = _mapper.Map<Domain.Models.JobPost>(dto);
        newEntity.CountryExposureSetId = await _countryExposureSetService.GetOrCreateSetIdAsync(dto.CountryExposureCountryCodes);
        await _repository.AddAsync(newEntity);
        await _repository.SaveChangesAsync();
        return _mapper.Map<JobPostDto>(newEntity);
    }

    public async Task<JobPostDto> UpdateAsync(JobPostDto dto)
    {
        // Check if we should create a new version
        bool shouldUpdateVersion = dto.ShouldUpdateVersion ?? false;
        
        if (shouldUpdateVersion)
        {
            // Use versioning service to create new version with step assignment copying
            var nextVersion = await _versioningService.GetNextJobPostVersionAsync(dto.Name);
            return await _versioningService.CreateJobPostVersionAsync(dto, nextVersion);
        }
        else
        {
            // Simple update of existing version
            return await UpdateExistingVersionAsync(dto);
        }
    }

    private async Task<JobPostDto> UpdateExistingVersionAsync(JobPostDto dto)
    {
        var spec = new JobPostByNameAndVersionWithCountryExposuresSpec(dto.Name, dto.Version);
        var existingEntity = await _repository.FirstOrDefaultAsync(spec);
        
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"JobPost with name '{dto.Name}' and version '{dto.Version}' not found.");
        }

        existingEntity.MaxAmountOfCandidatesRestriction = dto.MaxAmountOfCandidatesRestriction;
        existingEntity.MinimumRequirements = dto.MinimumRequirements;
        existingEntity.ExperienceLevel = dto.ExperienceLevel;
        existingEntity.JobTitle = dto.JobTitle;
        existingEntity.JobType = dto.JobType;
        existingEntity.JobDescription = dto.JobDescription;
        existingEntity.PoliceReportRequired = dto.PoliceReportRequired;
        existingEntity.Status = dto.Status;
        existingEntity.OriginCountryCode = NormalizeOriginCountryCode(dto.OriginCountryCode);
        existingEntity.UpdatedAt = DateTimeOffset.UtcNow;
        existingEntity.CountryExposureSetId = await _countryExposureSetService.GetOrCreateSetIdAsync(dto.CountryExposureCountryCodes);

        await _repository.UpdateAsync(existingEntity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<JobPostDto>(existingEntity);
    }

    /// <summary>Empty or whitespace OriginCountryCode would violate FK to Country; use null instead.</summary>
    private static string? NormalizeOriginCountryCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool AreJobPostsEqual(JobPostDto existing, JobPostDto incoming)
    {
        return existing.JobTitle == incoming.JobTitle &&
               existing.JobType == incoming.JobType &&
               existing.ExperienceLevel == incoming.ExperienceLevel &&
               existing.MaxAmountOfCandidatesRestriction == incoming.MaxAmountOfCandidatesRestriction;
    }
    
    // Delete is orchestrated by JobPostOrchestrator (JobPost + Steps workflow)

    public async Task<bool> ExistsAsync(string name, int version)
    {
        var spec = new JobPostByNameAndVersionSpec(name, version);
        var entity = await _repository.FirstOrDefaultAsync(spec);
        return entity != null;
    }

    public async Task<PagedResult<JobPostWithCandidatesDto>> GetListWithCandidateCountsAsync(JobPostListQueryDto query)
    {
        // Get job posts using existing filtered endpoint logic
        var jobPosts = await GetListAsync(query);
        
        // Get candidate counts for all job posts
        var jobPostKeys = jobPosts.Items.Select(jp => (jp.Name, jp.Version)).ToList();
        var candidateCounts = await _candidateService.GetCandidateCountsAsync(jobPostKeys);
        
        var jobPostsWithCandidates = jobPosts.Items.Select(jp => new JobPostWithCandidatesDto
        {
            Name = jp.Name,
            Version = jp.Version,
            JobTitle = jp.JobTitle,
            JobType = jp.JobType,
            ExperienceLevel = jp.ExperienceLevel,
            JobDescription = jp.JobDescription,
            PoliceReportRequired = jp.PoliceReportRequired,
            MaxAmountOfCandidatesRestriction = jp.MaxAmountOfCandidatesRestriction,
            MinimumRequirements = jp.MinimumRequirements,
            Status = jp.Status,
            OriginCountryCode = jp.OriginCountryCode,
            CountryExposureCountryCodes = jp.CountryExposureCountryCodes,
            CreatedAt = jp.CreatedAt,
            UpdatedAt = jp.UpdatedAt,
            AssignedSteps = jp.AssignedSteps,
            CandidateCount = candidateCounts.GetValueOrDefault($"{jp.Name}_{jp.Version}", 0)
        }).ToList();
        
        return new PagedResult<JobPostWithCandidatesDto>
        {
            Items = jobPostsWithCandidates,
            TotalCount = jobPosts.TotalCount,
            PageNumber = jobPosts.PageNumber,
            PageSize = jobPosts.PageSize
        };
    }

    public async Task<JobPostWithCandidatesDto?> GetByIdWithCandidatesAsync(string name, int version)
    {
        // Get job post
        var jobPost = await GetByIdAsync(name, version);
        if (jobPost == null)
            return null;

        // Get assigned steps directly
        var assignedSteps = await _stepAssignmentService.GetByJobPostAsync(name, version);
        jobPost.AssignedSteps = assignedSteps.ToList();

        // Get candidate count
        var candidateCounts = await _candidateService.GetCandidateCountsAsync(new[] { (name, version) });
        var candidateCount = candidateCounts.GetValueOrDefault($"{name}_{version}", 0);

        return new JobPostWithCandidatesDto
        {
            Name = jobPost.Name,
            Version = jobPost.Version,
            JobTitle = jobPost.JobTitle,
            JobType = jobPost.JobType,
            ExperienceLevel = jobPost.ExperienceLevel,
            JobDescription = jobPost.JobDescription,
            PoliceReportRequired = jobPost.PoliceReportRequired,
            MaxAmountOfCandidatesRestriction = jobPost.MaxAmountOfCandidatesRestriction,
            MinimumRequirements = jobPost.MinimumRequirements,
            Status = jobPost.Status,
            OriginCountryCode = jobPost.OriginCountryCode,
            CountryExposureCountryCodes = jobPost.CountryExposureCountryCodes,
            CreatedAt = jobPost.CreatedAt,
            UpdatedAt = jobPost.UpdatedAt,
            AssignedSteps = jobPost.AssignedSteps,
            CandidateCount = candidateCount
        };
    }
}