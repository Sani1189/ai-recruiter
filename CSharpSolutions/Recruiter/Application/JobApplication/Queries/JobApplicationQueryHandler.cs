using AutoMapper;
using Ardalis.Result;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.JobApplication.Specifications;

namespace Recruiter.Application.JobApplication.Queries;

/// Handles complex job application queries with pagination and filtering
public class JobApplicationQueryHandler
{
    private readonly IJobApplicationRepository _repository;
    private readonly IMapper _mapper;

    public JobApplicationQueryHandler(IJobApplicationRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    // Get job applications by candidate ID
    public async Task<Result<List<JobApplicationDto>>> GetByCandidateIdAsync(Guid candidateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new JobApplicationByCandidateIdSpec(candidateId);
            var jobApplications = await _repository.ListAsync(spec, cancellationToken);
            var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);
            return Result<List<JobApplicationDto>>.Success(jobApplicationDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<JobApplicationDto>>.Error();
        }
    }

    // Get job applications by job post
    public async Task<Result<List<JobApplicationDto>>> GetByJobPostAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new JobApplicationByJobPostSpec(jobPostName, jobPostVersion);
            var jobApplications = await _repository.ListAsync(spec, cancellationToken);
            var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);
            return Result<List<JobApplicationDto>>.Success(jobApplicationDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<JobApplicationDto>>.Error();
        }
    }

    // Get recent job applications
    public async Task<Result<List<JobApplicationDto>>> GetRecentJobApplicationsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var spec = new JobApplicationFilterSpec(new JobApplicationListQueryDto 
            { 
                AppliedAfter = cutoffDate,
                PageNumber = 1,
                PageSize = 100
            });
            var jobApplications = await _repository.ListAsync(spec, cancellationToken);
            var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);
            return Result<List<JobApplicationDto>>.Success(jobApplicationDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<JobApplicationDto>>.Error();
        }
    }

    // Get completed job applications
    public async Task<Result<List<JobApplicationDto>>> GetCompletedJobApplicationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new JobApplicationFilterSpec(new JobApplicationListQueryDto 
            { 
                IsCompleted = true,
                PageNumber = 1,
                PageSize = 100
            });
            var jobApplications = await _repository.ListAsync(spec, cancellationToken);
            var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);
            return Result<List<JobApplicationDto>>.Success(jobApplicationDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<JobApplicationDto>>.Error();
        }
    }

    // Advanced query with filtering, sorting, and pagination
    public async Task<Result<Common.Dto.PagedResult<JobApplicationDto>>> GetFilteredJobApplicationsAsync(JobApplicationListQueryDto query, CancellationToken cancellationToken = default)
    {
        // Validate query parameters
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            // Use specifications for complex queries
            var countSpec = new JobApplicationFilterCountSpec(query);
            var filterSpec = new JobApplicationFilterSpec(query);

            // Get total count efficiently
            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

            // Get filtered and paged results
            var jobApplications = await _repository.ListAsync(filterSpec, cancellationToken);

            // Map to DTOs
            var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);

            var result = new Common.Dto.PagedResult<JobApplicationDto>
            {
                Items = jobApplicationDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<JobApplicationDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<JobApplicationDto>>.Error();
        }
    }

    // Candidate-scoped filtered query (enforces CandidateId)
    public async Task<Result<Common.Dto.PagedResult<JobApplicationDto>>> GetMyFilteredJobApplicationsAsync(Guid candidateId, JobApplicationListQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            // Force the candidate ID to ensure we only get applications for this candidate
            query.CandidateId = candidateId;
           
            var countSpec = new JobApplicationFilterCountSpec(query);
            var filterSpec = new JobApplicationFilterSpec(query);

            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);
            var jobApplications = await _repository.ListAsync(filterSpec, cancellationToken);
            var jobApplicationDtos = _mapper.Map<List<JobApplicationDto>>(jobApplications);

            var result = new Common.Dto.PagedResult<JobApplicationDto>
            {
                Items = jobApplicationDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<JobApplicationDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<JobApplicationDto>>.Error();
        }
    }
}