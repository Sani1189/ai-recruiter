using Ardalis.Result;
using AutoMapper;
using Recruiter.Application.File.Dto;
using Recruiter.Application.File.Specifications;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.Application.File.Queries;

// File query handler using Ardalis specification pattern
public class FileQueryHandler
{
    private readonly IRepository<Domain.Models.File> _repository;
    private readonly IMapper _mapper;

    public FileQueryHandler(IRepository<Domain.Models.File> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Get file by ID using specification pattern
    public async Task<Result<FileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result<FileDto>.Invalid(new ValidationError { ErrorMessage = "Invalid file ID" });

        var spec = new FileByIdSpec(id);
        var file = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (file == null)
            return Result<FileDto>.NotFound($"File with ID {id} not found");

        var fileDto = _mapper.Map<FileDto>(file);
        return Result<FileDto>.Success(fileDto);
    }

    // Get files by container using specification pattern
    public async Task<Result<List<FileDto>>> GetByContainerAsync(string container, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(container))
            return Result<List<FileDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid container" });

        var spec = new FilesByContainerSpec(container);
        var files = await _repository.ListAsync(spec, cancellationToken);
        var fileDtos = _mapper.Map<List<FileDto>>(files);
        
        return Result<List<FileDto>>.Success(fileDtos);
    }

    // Get files by extension using specification pattern
    public async Task<Result<List<FileDto>>> GetByExtensionAsync(string extension, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return Result<List<FileDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid extension" });

        var spec = new FilesByExtensionSpec(extension);
        var files = await _repository.ListAsync(spec, cancellationToken);
        var fileDtos = _mapper.Map<List<FileDto>>(files);
        
        return Result<List<FileDto>>.Success(fileDtos);
    }

    // Get files by size using specification pattern
    public async Task<Result<List<FileDto>>> GetBySizeAsync(int minSizeMb = 1, CancellationToken cancellationToken = default)
    {
        var spec = new FilesBySizeSpec(minSizeMb);
        var files = await _repository.ListAsync(spec, cancellationToken);
        var fileDtos = _mapper.Map<List<FileDto>>(files);
        
        return Result<List<FileDto>>.Success(fileDtos);
    }

    // Get files by storage account using specification pattern
    public async Task<Result<List<FileDto>>> GetByStorageAccountAsync(string storageAccountName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageAccountName))
            return Result<List<FileDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid storage account name" });

        var spec = new FilesByStorageAccountSpec(storageAccountName);
        var files = await _repository.ListAsync(spec, cancellationToken);
        var fileDtos = _mapper.Map<List<FileDto>>(files);
        
        return Result<List<FileDto>>.Success(fileDtos);
    }

    // Advanced query with filtering, sorting, and pagination using Ardalis specifications
    public async Task<Result<Common.Dto.PagedResult<FileDto>>> GetFilteredFilesAsync(FileListQueryDto query, CancellationToken cancellationToken = default)
    {
        // Validate query parameters
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            // Use specifications for complex queries
            var countSpec = new FileFilterCountSpec(query);
            var filterSpec = new FileFilterSpec(query);

            // Get total count efficiently
            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

            // Get filtered and paged results
            var files = await _repository.ListAsync(filterSpec, cancellationToken);

            // Map to DTOs
            var fileDtos = _mapper.Map<List<FileDto>>(files);

            var result = new Common.Dto.PagedResult<FileDto>
            {
                Items = fileDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<FileDto>>.Success(result);
        }
        catch (Exception ex)
        {
            // Log exception (not shown here)
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<FileDto>>.Error();
        }
    }
}
