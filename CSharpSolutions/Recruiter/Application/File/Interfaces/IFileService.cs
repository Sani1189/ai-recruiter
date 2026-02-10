using Recruiter.Application.File.Dto;
using Ardalis.Result;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.File.Interfaces;

public interface IFileService
{
    Task<IEnumerable<FileDto>> GetAllAsync();
    Task<FileDto?> GetByIdAsync(Guid id);
    Task<FileDto> CreateAsync(FileDto dto);
    Task<FileDto> UpdateAsync(FileDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<FileDto>>> GetByContainerAsync(string container, CancellationToken cancellationToken = default);
    Task<Result<List<FileDto>>> GetByExtensionAsync(string extension, CancellationToken cancellationToken = default);
    Task<Result<List<FileDto>>> GetBySizeAsync(int minSizeMb = 1, CancellationToken cancellationToken = default);
    Task<Result<List<FileDto>>> GetByStorageAccountAsync(string storageAccountName, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<FileDto>>> GetFilteredFilesAsync(FileListQueryDto query, CancellationToken cancellationToken = default);
}
