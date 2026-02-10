using Ardalis.Result;
using AutoMapper;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Queries;

// JobPost query handler using Ardalis specification pattern
public class JobPostQueryHandler
{
    private readonly IRepository<Domain.Models.JobPost> _repository;
    private readonly IRepository<Domain.Models.JobPostStep> _stepRepository;
    private readonly IMapper _mapper;

    public JobPostQueryHandler(IRepository<Domain.Models.JobPost> repository, IRepository<Domain.Models.JobPostStep> stepRepository, IMapper mapper)
    {
        _repository = repository;
        _stepRepository = stepRepository;
        _mapper = mapper;
    }

    // Get job post by name and version using specification pattern
    public async Task<Result<JobPostDto>> GetByIdAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<JobPostDto>.Invalid(new ValidationError { ErrorMessage = "Job post name cannot be empty" });

        if (version < 1)
            return Result<JobPostDto>.Invalid(new ValidationError { ErrorMessage = "Version must be greater than 0" });

        var spec = new JobPostByNameAndVersionWithCountryExposuresSpec(name, version);
        var jobPost = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (jobPost == null)
            return Result<JobPostDto>.NotFound($"Job post '{name}' version {version} not found");

        var jobPostDto = _mapper.Map<JobPostDto>(jobPost);
        return Result<JobPostDto>.Success(jobPostDto);
    }

    // Get latest version of job post by name
    public async Task<Result<JobPostDto>> GetLatestByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<JobPostDto>.Invalid(new ValidationError { ErrorMessage = "Job post name cannot be empty" });

        var spec = new JobPostLatestByNameSpec(name);
        var jobPost = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (jobPost == null)
            return Result<JobPostDto>.NotFound($"Job post '{name}' not found");

        var jobPostDto = _mapper.Map<JobPostDto>(jobPost);
        return Result<JobPostDto>.Success(jobPostDto);
    }

    /// <summary>Get published job by name and version only (for public/candidate). Single query with Status filter.</summary>
    public async Task<Result<JobPostDto>> GetPublishedByIdAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<JobPostDto>.Invalid(new ValidationError { ErrorMessage = "Job post name cannot be empty" });
        if (version < 1)
            return Result<JobPostDto>.Invalid(new ValidationError { ErrorMessage = "Version must be greater than 0" });

        var spec = new JobPostByNameVersionAndPublishedSpec(name, version);
        var jobPost = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        if (jobPost == null)
            return Result<JobPostDto>.NotFound();

        return Result<JobPostDto>.Success(_mapper.Map<JobPostDto>(jobPost));
    }

    /// <summary>Get latest published version by name only (for public/candidate). Single query with Status filter.</summary>
    public async Task<Result<JobPostDto>> GetLatestPublishedByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<JobPostDto>.Invalid(new ValidationError { ErrorMessage = "Job post name cannot be empty" });

        var spec = new JobPostLatestPublishedByNameSpec(name);
        var jobPost = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        if (jobPost == null)
            return Result<JobPostDto>.NotFound();

        return Result<JobPostDto>.Success(_mapper.Map<JobPostDto>(jobPost));
    }

    // Get edit history by name
    public async Task<Result<List<JobPostDto>>> GetEditHistoryAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<List<JobPostDto>>.Invalid(new ValidationError { ErrorMessage = "Job post name cannot be empty" });

        var spec = new JobPostByNameSpec(name);
        var jobPosts = await _repository.ListAsync(spec, cancellationToken);
        var jobPostDtos = _mapper.Map<List<JobPostDto>>(jobPosts);
        
        return Result<List<JobPostDto>>.Success(jobPostDtos);
    }

    // Get job posts by experience level
    public async Task<Result<List<JobPostDto>>> GetByExperienceLevelAsync(string experienceLevel, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(experienceLevel))
            return Result<List<JobPostDto>>.Invalid(new ValidationError { ErrorMessage = "Experience level cannot be empty" });

        var spec = new AllJobPostsSpec();
        var jobPosts = await _repository.ListAsync(spec, cancellationToken);
        
        // Filter by experience level
        var filtered = jobPosts.Where(jp => jp.ExperienceLevel == experienceLevel).ToList();
        var jobPostDtos = _mapper.Map<List<JobPostDto>>(filtered);
        
        return Result<List<JobPostDto>>.Success(jobPostDtos);
    }

    // Get job posts by job type
    public async Task<Result<List<JobPostDto>>> GetByJobTypeAsync(string jobType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobType))
            return Result<List<JobPostDto>>.Invalid(new ValidationError { ErrorMessage = "Job type cannot be empty" });

        var spec = new AllJobPostsSpec();
        var jobPosts = await _repository.ListAsync(spec, cancellationToken);
        
        // Filter by job type
        var filtered = jobPosts.Where(jp => jp.JobType == jobType).ToList();
        var jobPostDtos = _mapper.Map<List<JobPostDto>>(filtered);
        
        return Result<List<JobPostDto>>.Success(jobPostDtos);
    }

    // Get all job posts
    public async Task<Result<List<JobPostDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var spec = new AllJobPostsSpec();
        var jobPosts = await _repository.ListAsync(spec, cancellationToken);
        var jobPostDtos = _mapper.Map<List<JobPostDto>>(jobPosts);
        
        return Result<List<JobPostDto>>.Success(jobPostDtos);
    }

    // Advanced query with filtering, sorting, and pagination
    public async Task<Result<Recruiter.Application.Common.Dto.PagedResult<JobPostDto>>> GetFilteredAsync(JobPostListQueryDto query, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new AllJobPostsSpec();
            var jobPosts = await _repository.ListAsync(spec, cancellationToken);
            
            // Apply filters
            var filtered = jobPosts.AsQueryable();
            
            if (!string.IsNullOrEmpty(query.Name))
                filtered = filtered.Where(x => x.Name.Contains(query.Name));
            
            if (query.Version.HasValue)
                filtered = filtered.Where(x => x.Version == query.Version.Value);
            
            if (!string.IsNullOrEmpty(query.ExperienceLevel))
                filtered = filtered.Where(x => x.ExperienceLevel == query.ExperienceLevel);
            
            if (!string.IsNullOrEmpty(query.JobTitle))
                filtered = filtered.Where(x => x.JobTitle.Contains(query.JobTitle));
            
            if (!string.IsNullOrEmpty(query.JobType))
                filtered = filtered.Where(x => x.JobType == query.JobType);
            
            if (query.PoliceReportRequired.HasValue)
                filtered = filtered.Where(x => x.PoliceReportRequired == query.PoliceReportRequired.Value);
            
            if (query.CreatedAfter.HasValue)
                filtered = filtered.Where(x => x.CreatedAt >= query.CreatedAfter.Value);
            
            if (query.CreatedBefore.HasValue)
                filtered = filtered.Where(x => x.CreatedAt <= query.CreatedBefore.Value);
            
            if (!string.IsNullOrEmpty(query.CreatedBy))
                filtered = filtered.Where(x => x.CreatedBy == query.CreatedBy);

            // Apply search term filter (searches across multiple fields)
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.ToLower();
                filtered = filtered.Where(x => 
                    x.Name.ToLower().Contains(searchTerm) ||
                    x.JobTitle.ToLower().Contains(searchTerm) ||
                    x.JobDescription.ToLower().Contains(searchTerm) ||
                    x.JobType.ToLower().Contains(searchTerm));
            }

            // Apply sorting
            filtered = query.SortBy?.ToLower() switch
            {
                "name" => query.SortDescending ? filtered.OrderByDescending(x => x.Name) : filtered.OrderBy(x => x.Name),
                "version" => query.SortDescending ? filtered.OrderByDescending(x => x.Version) : filtered.OrderBy(x => x.Version),
                "jobtitle" => query.SortDescending ? filtered.OrderByDescending(x => x.JobTitle) : filtered.OrderBy(x => x.JobTitle),
                "jobtype" => query.SortDescending ? filtered.OrderByDescending(x => x.JobType) : filtered.OrderBy(x => x.JobType),
                "experiencelevel" => query.SortDescending ? filtered.OrderByDescending(x => x.ExperienceLevel) : filtered.OrderBy(x => x.ExperienceLevel),
                "updatedat" => query.SortDescending ? filtered.OrderByDescending(x => x.UpdatedAt) : filtered.OrderBy(x => x.UpdatedAt),
                _ => query.SortDescending ? filtered.OrderByDescending(x => x.CreatedAt) : filtered.OrderBy(x => x.CreatedAt)
            };

            // Get total count before pagination
            var totalCount = filtered.Count();

            // Apply pagination
            var paged = filtered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var jobPostDtos = _mapper.Map<List<JobPostDto>>(paged);
            
            var result = new Recruiter.Application.Common.Dto.PagedResult<JobPostDto>
            {
                Items = jobPostDtos,
                TotalCount = totalCount,
                PageNumber = query.Page,
                PageSize = query.PageSize
            };

            return Result<Recruiter.Application.Common.Dto.PagedResult<JobPostDto>>.Success(result);
        }
        catch (Exception)
        {
            return Result<Recruiter.Application.Common.Dto.PagedResult<JobPostDto>>.Error();
        }
    }

    // JobPostStep query methods
    public async Task<Result<JobPostStepDto>> GetStepByIdAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<JobPostStepDto>.Invalid(new ValidationError { ErrorMessage = "Job post step name cannot be empty" });

        if (version < 1)
            return Result<JobPostStepDto>.Invalid(new ValidationError { ErrorMessage = "Version must be greater than 0" });

        var spec = new JobPostStepByNameAndVersionSpec(name, version);
        var jobPostStep = await _stepRepository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (jobPostStep == null)
            return Result<JobPostStepDto>.NotFound($"Job post step '{name}' version {version} not found");

        var jobPostStepDto = _mapper.Map<JobPostStepDto>(jobPostStep);
        return Result<JobPostStepDto>.Success(jobPostStepDto);
    }

    public async Task<Result<JobPostStepDto>> GetStepLatestByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<JobPostStepDto>.Invalid(new ValidationError { ErrorMessage = "Job post step name cannot be empty" });

        var spec = new JobPostStepLatestByNameSpec(name);
        var jobPostStep = await _stepRepository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (jobPostStep == null)
            return Result<JobPostStepDto>.NotFound($"Job post step '{name}' not found");

        var jobPostStepDto = _mapper.Map<JobPostStepDto>(jobPostStep);
        return Result<JobPostStepDto>.Success(jobPostStepDto);
    }

    public async Task<Result<Recruiter.Application.Common.Dto.PagedResult<JobPostStepDto>>> GetStepListAsync(JobPostStepQueryDto query, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new AllJobPostStepsSpec();
            var jobPostSteps = await _stepRepository.ListAsync(spec, cancellationToken);
            
            // Apply filters
            var filtered = jobPostSteps.AsQueryable();
            
            if (!string.IsNullOrEmpty(query.Name))
                filtered = filtered.Where(x => x.Name.Contains(query.Name));
            
            if (query.Version.HasValue)
                filtered = filtered.Where(x => x.Version == query.Version.Value);
            
            if (!string.IsNullOrEmpty(query.StepType))
                filtered = filtered.Where(x => x.StepType == query.StepType);
            
            if (query.CreatedAt.HasValue)
                filtered = filtered.Where(x => x.CreatedAt >= query.CreatedAt.Value);
            
            if (query.UpdatedAt.HasValue)
                filtered = filtered.Where(x => x.UpdatedAt <= query.UpdatedAt.Value);
            
            if (!string.IsNullOrEmpty(query.CreatedBy))
                filtered = filtered.Where(x => x.CreatedBy == query.CreatedBy);

            // Apply search term filter (searches across multiple fields)
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.ToLower();
                filtered = filtered.Where(x => 
                    x.Name.ToLower().Contains(searchTerm) ||
                    x.StepType.ToLower().Contains(searchTerm) ||
                    (x.InterviewConfigurationName != null && x.InterviewConfigurationName.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            filtered = query.SortBy?.ToLower() switch
            {
                "name" => query.SortDescending ? filtered.OrderByDescending(x => x.Name) : filtered.OrderBy(x => x.Name),
                "version" => query.SortDescending ? filtered.OrderByDescending(x => x.Version) : filtered.OrderBy(x => x.Version),
                "steptype" => query.SortDescending ? filtered.OrderByDescending(x => x.StepType) : filtered.OrderBy(x => x.StepType),
                "updatedat" => query.SortDescending ? filtered.OrderByDescending(x => x.UpdatedAt) : filtered.OrderBy(x => x.UpdatedAt),
                _ => query.SortDescending ? filtered.OrderByDescending(x => x.CreatedAt) : filtered.OrderBy(x => x.CreatedAt)
            };

            // Get total count before pagination
            var totalCount = filtered.Count();

            // Apply pagination
            var paged = filtered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var jobPostStepDtos = _mapper.Map<List<JobPostStepDto>>(paged);
            
            var result = new Recruiter.Application.Common.Dto.PagedResult<JobPostStepDto>
            {
                Items = jobPostStepDtos,
                TotalCount = totalCount,
                PageNumber = query.Page,
                PageSize = query.PageSize
            };

            return Result<Recruiter.Application.Common.Dto.PagedResult<JobPostStepDto>>.Success(result);
        }
        catch (Exception)
        {
            return Result<Recruiter.Application.Common.Dto.PagedResult<JobPostStepDto>>.Error();
        }
    }

    public async Task<Result<List<JobPostStepDto>>> GetStepDropdownListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new JobPostStepLatestVersionsSpec();
            var jobPostSteps = await _stepRepository.ListAsync(spec, cancellationToken);
            
            // Group by name
            var latestVersions = jobPostSteps
                .GroupBy(jps => jps.Name)
                .Select(g => g.First())
                .ToList();
            
            var jobPostStepDtos = _mapper.Map<List<JobPostStepDto>>(latestVersions);
            return Result<List<JobPostStepDto>>.Success(jobPostStepDtos);
        }
        catch (Exception)
        {
            return Result<List<JobPostStepDto>>.Error();
        }
    }
}