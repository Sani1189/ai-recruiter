using Ardalis.Result;
using FluentValidation;
using Recruiter.Application.File.Dto;
using Recruiter.Application.File.Interfaces;

namespace Recruiter.Application.File;

// File Orchestrator for complex business operations
public interface IFileOrchestrator
{
    Task<Result<FileDto>> GetFileWithMetadataAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<FileDto>> ProcessFileAsync(FileDto fileDto, CancellationToken cancellationToken = default);
    Task<Result<FileDto>> UpdateFileMetadataAsync(Guid id, string type, CancellationToken cancellationToken = default);
}

// File Orchestrator implementation
public class FileOrchestrator : IFileOrchestrator
{
    private readonly IFileService _fileService;
    private readonly IValidator<FileDto> _fileValidator;

    public FileOrchestrator(
        IFileService fileService,
        IValidator<FileDto> fileValidator)
    {
        _fileService = fileService;
        _fileValidator = fileValidator;
    }

    public async Task<Result<FileDto>> GetFileWithMetadataAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await _fileService.GetByIdAsync(id);
        if (file == null)
        {
            return Result<FileDto>.NotFound($"File with ID {id} not found");
        }

        // Get related metadata would be handled here
        // This is a simple implementation - in real scenario you might want to include additional metadata

        return Result<FileDto>.Success(file);
    }

    public async Task<Result<FileDto>> ProcessFileAsync(FileDto fileDto, CancellationToken cancellationToken = default)
    {
        // Validate the file
        var validationResult = await _fileValidator.ValidateAsync(fileDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<FileDto>.Invalid(validationResult.Errors.Select(e => new ValidationError { ErrorMessage = e.ErrorMessage }).ToArray());
        }

        // Process the file (create or update)
        FileDto result;
        if (fileDto.Id == Guid.Empty)
        {
            result = await _fileService.CreateAsync(fileDto);
        }
        else
        {
            result = await _fileService.UpdateAsync(fileDto);
        }

        return Result<FileDto>.Success(result);
    }

    public async Task<Result<FileDto>> UpdateFileMetadataAsync(Guid id, string type, CancellationToken cancellationToken = default)
    {
        var file = await _fileService.GetByIdAsync(id);
        if (file == null)
        {
            return Result<FileDto>.NotFound($"File with ID {id} not found");
        }

        // Update file extension (closest to type)
        file.Extension = type;
        var updatedFile = await _fileService.UpdateAsync(file);

        return Result<FileDto>.Success(updatedFile);
    }
}
