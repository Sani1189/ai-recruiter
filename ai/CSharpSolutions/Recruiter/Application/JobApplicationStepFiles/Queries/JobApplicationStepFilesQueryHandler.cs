using Ardalis.Result;
using AutoMapper;
using Recruiter.Application.JobApplicationStepFiles.Dto;
using Recruiter.Application.JobApplicationStepFiles.Specifications;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobApplicationStepFiles.Queries;

public class JobApplicationStepFilesQueryHandler
{
    private readonly IRepository<Domain.Models.JobApplicationStepFiles> _repository;
    private readonly IMapper _mapper;

    public JobApplicationStepFilesQueryHandler(IRepository<Domain.Models.JobApplicationStepFiles> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Get by ID
    public async Task<Result<JobApplicationStepFilesDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result<JobApplicationStepFilesDto>.Invalid(new ValidationError { ErrorMessage = "Invalid ID" });

        var spec = new JobApplicationStepFilesByIdSpec(id);
        var jobApplicationStepFile = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (jobApplicationStepFile == null)
            return Result<JobApplicationStepFilesDto>.NotFound($"Job application step file with ID {id} not found");

        var jobApplicationStepFileDto = _mapper.Map<JobApplicationStepFilesDto>(jobApplicationStepFile);
        return Result<JobApplicationStepFilesDto>.Success(jobApplicationStepFileDto);
    }

    // Get by file ID
    public async Task<Result<List<JobApplicationStepFilesDto>>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        if (fileId == Guid.Empty)
            return Result<List<JobApplicationStepFilesDto>>.Invalid(new ValidationError { ErrorMessage = "File ID cannot be empty" });

        var spec = new JobApplicationStepFilesByFileIdSpec(fileId);
        var jobApplicationStepFiles = await _repository.ListAsync(spec, cancellationToken);
        var jobApplicationStepFileDtos = _mapper.Map<List<JobApplicationStepFilesDto>>(jobApplicationStepFiles);
        
        return Result<List<JobApplicationStepFilesDto>>.Success(jobApplicationStepFileDtos);
    }

    // Get by job application step ID
    public async Task<Result<List<JobApplicationStepFilesDto>>> GetByJobApplicationStepIdAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default)
    {
        if (jobApplicationStepId == Guid.Empty)
            return Result<List<JobApplicationStepFilesDto>>.Invalid(new ValidationError { ErrorMessage = "Job application step ID cannot be empty" });

        var spec = new JobApplicationStepFilesByJobApplicationStepIdSpec(jobApplicationStepId);
        var jobApplicationStepFiles = await _repository.ListAsync(spec, cancellationToken);
        var jobApplicationStepFileDtos = _mapper.Map<List<JobApplicationStepFilesDto>>(jobApplicationStepFiles);
        
        return Result<List<JobApplicationStepFilesDto>>.Success(jobApplicationStepFileDtos);
    }

    // Check if file is already attached to step
    public async Task<Result<bool>> IsFileAttachedToStepAsync(Guid fileId, Guid jobApplicationStepId, CancellationToken cancellationToken = default)
    {
        if (fileId == Guid.Empty || jobApplicationStepId == Guid.Empty)
            return Result<bool>.Invalid(new ValidationError { ErrorMessage = "File ID and Job application step ID cannot be empty" });

        var spec = new JobApplicationStepFilesByFileAndStepSpec(fileId, jobApplicationStepId);
        var existing = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        return Result<bool>.Success(existing != null);
    }

    // Get filtered job application step files with pagination
    public async Task<Result<Common.Dto.PagedResult<JobApplicationStepFilesDto>>> GetFilteredJobApplicationStepFilesAsync(JobApplicationStepFilesListQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            var countSpec = new JobApplicationStepFilesFilterCountSpec(query);
            var filterSpec = new JobApplicationStepFilesFilterSpec(query);

            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);
            var jobApplicationStepFiles = await _repository.ListAsync(filterSpec, cancellationToken);

            var jobApplicationStepFileDtos = _mapper.Map<List<JobApplicationStepFilesDto>>(jobApplicationStepFiles);

            var result = new Common.Dto.PagedResult<JobApplicationStepFilesDto>
            {
                Items = jobApplicationStepFileDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<JobApplicationStepFilesDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<JobApplicationStepFilesDto>>.Error();
        }
    }
}
