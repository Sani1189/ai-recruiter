using AutoMapper;
using Ardalis.Result;
using Recruiter.Application.Interview.Dto;
using Recruiter.Application.Interview.Interfaces;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.Interview.Specifications;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.Application.Interview.Queries;

/// Handles complex interview queries with pagination and filtering
public class InterviewQueryHandler
{
    private readonly IRepository<Domain.Models.Interview> _repository;
    private readonly IMapper _mapper;

    public InterviewQueryHandler(IRepository<Domain.Models.Interview> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    // Get interviews by job application step ID
    public async Task<Result<List<InterviewDto>>> GetByJobApplicationStepIdAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new InterviewByJobApplicationStepIdSpec(jobApplicationStepId);
            var interviews = await _repository.ListAsync(spec, cancellationToken);
            var interviewDtos = _mapper.Map<List<InterviewDto>>(interviews);
            return Result<List<InterviewDto>>.Success(interviewDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<InterviewDto>>.Error();
        }
    }

    // Get interviews by configuration
    public async Task<Result<List<InterviewDto>>> GetByConfigurationAsync(string configurationName, int configurationVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new InterviewByConfigurationSpec(configurationName, configurationVersion);
            var interviews = await _repository.ListAsync(spec, cancellationToken);
            var interviewDtos = _mapper.Map<List<InterviewDto>>(interviews);
            return Result<List<InterviewDto>>.Success(interviewDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<InterviewDto>>.Error();
        }
    }

    // Get recent interviews
    public async Task<Result<List<InterviewDto>>> GetRecentInterviewsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var spec = new InterviewFilterSpec(new InterviewListQueryDto 
            { 
                CompletedAfter = cutoffDate,
                PageNumber = 1,
                PageSize = 100
            });
            var interviews = await _repository.ListAsync(spec, cancellationToken);
            var interviewDtos = _mapper.Map<List<InterviewDto>>(interviews);
            return Result<List<InterviewDto>>.Success(interviewDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<InterviewDto>>.Error();
        }
    }

    // Get completed interviews
    public async Task<Result<List<InterviewDto>>> GetCompletedInterviewsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new InterviewFilterSpec(new InterviewListQueryDto 
            { 
                IsCompleted = true,
                PageNumber = 1,
                PageSize = 100
            });
            var interviews = await _repository.ListAsync(spec, cancellationToken);
            var interviewDtos = _mapper.Map<List<InterviewDto>>(interviews);
            return Result<List<InterviewDto>>.Success(interviewDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<InterviewDto>>.Error();
        }
    }

    // Advanced query with filtering, sorting, and pagination
    public async Task<Result<Common.Dto.PagedResult<InterviewDto>>> GetFilteredInterviewsAsync(InterviewListQueryDto query, CancellationToken cancellationToken = default)
    {
        // Validate query parameters
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            // Use specifications for complex queries
            var countSpec = new InterviewFilterCountSpec(query);
            var filterSpec = new InterviewFilterSpec(query);

            // Get total count efficiently
            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

            // Get filtered and paged results
            var interviews = await _repository.ListAsync(filterSpec, cancellationToken);

            // Map to DTOs
            var interviewDtos = _mapper.Map<List<InterviewDto>>(interviews);

            var result = new Common.Dto.PagedResult<InterviewDto>
            {
                Items = interviewDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<InterviewDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<InterviewDto>>.Error();
        }
    }
}