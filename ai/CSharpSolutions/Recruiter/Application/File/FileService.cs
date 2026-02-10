using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.File.Dto;
using Recruiter.Application.File.Interfaces;
using Recruiter.Domain.Models;
using Ardalis.Result;

namespace Recruiter.Application.File;

public class FileService : IFileService
{
    private readonly IRepository<Domain.Models.File> _repository;
    private readonly IMapper _mapper;
    private readonly Queries.FileQueryHandler _queryHandler;

    public FileService(IRepository<Domain.Models.File> repository, IMapper mapper, Queries.FileQueryHandler queryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    public async Task<IEnumerable<FileDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<FileDto>>(entities);
    }

    public async Task<FileDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<FileDto>(entity) : null;
    }

    public async Task<FileDto> CreateAsync(FileDto dto)
    {
        var entity = _mapper.Map<Domain.Models.File>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<FileDto>(entity);
    }

    public async Task<FileDto> UpdateAsync(FileDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"File with id '{dto.Id}' not found.");
        }

        // Map DTO to entity
        var updatedEntity = _mapper.Map<Domain.Models.File>(dto);
        updatedEntity.Id = entity.Id; // Preserve the ID

        await _repository.UpdateAsync(updatedEntity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<FileDto>(updatedEntity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity != null)
        {
            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null;
    }

    // Delegate complex queries to QueryHandler
    public async Task<Result<List<FileDto>>> GetByContainerAsync(string container, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByContainerAsync(container, cancellationToken);
    }

    public async Task<Result<List<FileDto>>> GetByExtensionAsync(string extension, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByExtensionAsync(extension, cancellationToken);
    }

    public async Task<Result<List<FileDto>>> GetBySizeAsync(int minSizeMb = 1, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetBySizeAsync(minSizeMb, cancellationToken);
    }

    public async Task<Result<List<FileDto>>> GetByStorageAccountAsync(string storageAccountName, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByStorageAccountAsync(storageAccountName, cancellationToken);
    }

    public async Task<Result<Common.Dto.PagedResult<FileDto>>> GetFilteredFilesAsync(FileListQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredFilesAsync(query, cancellationToken);
    }
}
