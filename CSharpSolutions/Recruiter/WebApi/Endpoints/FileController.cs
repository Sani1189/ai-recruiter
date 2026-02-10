using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Text.RegularExpressions;
using Recruiter.Application.File.Dto;
using Recruiter.Application.File.Interfaces;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Options;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrCandidate")]
public class FileController(
    IFileService fileService,
    IFileStorageService fileStorageService,
    AzureStorageOptions storageOptions) : ControllerBase
{
    private readonly IFileService _fileService = fileService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly AzureStorageOptions _storageOptions = storageOptions;

    private static readonly Regex UnsafePathChars = new(@"[^a-zA-Z0-9_\-\.]+", RegexOptions.Compiled);

    /// <summary>
    /// Get a secure upload URL (SAS) for direct client-to-Azure uploads.
    /// Intended for admin/recruiter authoring workflows (e.g., assessment template media).
    /// </summary>
    [HttpPost("upload/get-upload-url")]
    [Authorize(Policy = "Admin")]
    public ActionResult<GetUploadUrlResponseDto> GetUploadUrl([FromBody] GetUploadUrlRequestDto request, [FromQuery] int expirationMinutes = 20)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (expirationMinutes is < 1 or > 60)
        {
            return BadRequest(new { message = "expirationMinutes must be between 1 and 60." });
        }

        var container = string.IsNullOrWhiteSpace(request.Container) ? _storageOptions.ContainerName : request.Container.Trim();
        if (string.IsNullOrWhiteSpace(container))
        {
            return BadRequest(new { message = "Storage container is not configured." });
        }

        var originalFileName = request.FileName.Trim();
        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".bin";
        }

        // Folder prefix within the container (safe)
        var safePrefix = string.IsNullOrWhiteSpace(request.FolderPrefix)
            ? "uploads"
            : UnsafePathChars.Replace(request.FolderPrefix.Trim(), "-").Trim('-');

        var folderPath = $"{safePrefix}/{DateTime.UtcNow:yyyy/MM}";

        // Generate unique blob file name
        var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var blobPath = $"{folderPath}/{safeFileName}";

        var uploadUrl = _fileStorageService.GenerateUploadSasUrl(container, blobPath, expirationMinutes);
        var storageAccountName = _fileStorageService.GetStorageAccountName();
        var blobUrl = $"https://{storageAccountName}.blob.core.windows.net/{container}/{blobPath}";

        return Ok(new GetUploadUrlResponseDto
        {
            UploadUrl = uploadUrl,
            Container = container,
            FolderPath = folderPath,
            FilePath = safeFileName,
            StorageAccountName = storageAccountName,
            BlobUrl = blobUrl,
            ExpiresInMinutes = expirationMinutes
        });
    }

    /// <summary>
    /// Get file by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FileDto>> GetFileById(Guid id)
    {
        var file = await _fileService.GetByIdAsync(id);
        if (file == null)
            return NotFound(new { message = $"File with ID {id} not found" });
        return Ok(file);
    }

    /// <summary>
    /// Get all files
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileDto>>> GetAllFiles()
    {
        var files = await _fileService.GetAllAsync();
        return Ok(files);
    }

    /// <summary>
    /// Get files by container name
    /// </summary>
    [HttpGet("container/{container}")]
    public async Task<ActionResult<List<FileDto>>> GetFilesByContainer(string container)
    {
        var result = await _fileService.GetByContainerAsync(container);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get files by file extension
    /// </summary>
    [HttpGet("extension/{extension}")]
    public async Task<ActionResult<List<FileDto>>> GetFilesByExtension(string extension)
    {
        var result = await _fileService.GetByExtensionAsync(extension);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get files by size (minimum size in MB)
    /// </summary>
    [HttpGet("size")]
    public async Task<ActionResult<List<FileDto>>> GetFilesBySize([FromQuery] int minSizeMb = 1)
    {
        var result = await _fileService.GetBySizeAsync(minSizeMb);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get files by storage account name
    /// </summary>
    [HttpGet("storage-account/{storageAccountName}")]
    public async Task<ActionResult<List<FileDto>>> GetFilesByStorageAccount(string storageAccountName)
    {
        var result = await _fileService.GetByStorageAccountAsync(storageAccountName);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get filtered files with pagination and sorting
    /// </summary>
    [HttpPost("filter")]
    public async Task<ActionResult<Application.Common.Dto.PagedResult<FileDto>>> GetFilteredFiles([FromBody] FileListQueryDto query)
    {
        var result = await _fileService.GetFilteredFilesAsync(query);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new file
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FileDto>> CreateFile([FromBody] FileDto fileDto)
    {
        if (fileDto == null)
            return BadRequest("File data is required");

        var createdFile = await _fileService.CreateAsync(fileDto);
        return CreatedAtAction(nameof(GetFileById), new { id = createdFile.Id }, createdFile);
    }

    /// <summary>
    /// Update an existing file
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FileDto>> UpdateFile(Guid id, [FromBody] FileDto fileDto)
    {
        if (fileDto == null)
            return BadRequest("File data is required");

        fileDto.Id = id;

        try
        {
            var updatedFile = await _fileService.UpdateAsync(fileDto);
            return Ok(updatedFile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFile(Guid id)
    {
        var file = await _fileService.GetByIdAsync(id);
        if (file == null)
            return NotFound(new { message = $"File with ID {id} not found" });

        await _fileService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Get secure download URL for a file (short-lived, read-once style)
    /// </summary>
    [HttpGet("{id}/download-url")]
    public async Task<IActionResult> GetDownloadUrl(Guid id, [FromQuery] int expirationMinutes = 5)
    {
        var file = await _fileService.GetByIdAsync(id);
        if (file == null)
            return NotFound(new { message = $"File with ID {id} not found" });

        try
        {
            var blobPath = string.IsNullOrWhiteSpace(file.FolderPath) 
                ? file.FilePath 
                : $"{file.FolderPath.TrimEnd('/')}/{file.FilePath}";
            var containerName = string.IsNullOrWhiteSpace(file.Container) ? _storageOptions.ContainerName : file.Container;

            var downloadUrl = _fileStorageService.GenerateSecureDownloadUrl(containerName, blobPath, expirationMinutes);
            return Ok(new { downloadUrl, expiresInMinutes = expirationMinutes });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating download URL", error = ex.Message });
        }
    }

    /// <summary>
    /// Get secure download URL for a file (short-lived, read-once style)
    /// This endpoint replaces the old streaming download for better performance and security
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadFile(Guid id, [FromQuery] int expirationMinutes = 5)
    {
        var file = await _fileService.GetByIdAsync(id);
        if (file == null)
            return NotFound(new { message = $"File with ID {id} not found" });

        try
        {
            var blobPath = string.IsNullOrWhiteSpace(file.FolderPath) 
                ? file.FilePath 
                : $"{file.FolderPath.TrimEnd('/')}/{file.FilePath}";
            var containerName = string.IsNullOrWhiteSpace(file.Container) ? _storageOptions.ContainerName : file.Container;

            var downloadUrl = _fileStorageService.GenerateSecureDownloadUrl(containerName, blobPath, expirationMinutes);
            return Ok(new { downloadUrl, expiresInMinutes = expirationMinutes });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = "File not found", error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = "Access denied", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating download URL", error = ex.Message });
        }
    }
}
