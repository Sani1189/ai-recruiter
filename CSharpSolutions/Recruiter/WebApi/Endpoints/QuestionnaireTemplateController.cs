using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Import.Dto;
using Recruiter.Application.QuestionnaireTemplate.Import.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Ardalis.Result;
using DuplicateTemplateRequestDto = Recruiter.Application.QuestionnaireTemplate.Dto.DuplicateTemplateRequestDto;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
public class QuestionnaireTemplateController(
    IQuestionnaireTemplateService service,
    IQuestionnaireTemplateImportService importService,
    ILogger<QuestionnaireTemplateController> logger) : ControllerBase
{
    private readonly IQuestionnaireTemplateService _service = service;
    private readonly IQuestionnaireTemplateImportService _importService = importService;
    private readonly ILogger<QuestionnaireTemplateController> _logger = logger;

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<QuestionnaireTemplateDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    [HttpGet("filtered")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<Recruiter.Application.Common.Dto.PagedResult<QuestionnaireTemplateDto>>> GetFiltered([FromQuery] QuestionnaireTemplateListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _service.GetFilteredAsync(query, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return Ok(result.Value);
    }

    [HttpGet("{name}/{version:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<QuestionnaireTemplateDto>> GetById(string name, int version, CancellationToken cancellationToken)
    {
        var dto = await _service.GetByIdAsync(name, version, cancellationToken);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpGet("{name}/latest")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<QuestionnaireTemplateDto>> GetLatest(string name, CancellationToken cancellationToken)
    {
        var dto = await _service.GetLatestVersionAsync(name, cancellationToken);
        return dto == null ? NotFound() : Ok(dto);
    }

    [HttpGet("{name}/versions")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<IEnumerable<QuestionnaireTemplateDto>>> GetAllVersions(string name, CancellationToken cancellationToken)
    {
        var versions = await _service.GetAllVersionsAsync(name, cancellationToken);
        return Ok(versions);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<QuestionnaireTemplateDto>> Create([FromBody] QuestionnaireTemplateDto dto, CancellationToken cancellationToken)
    {
        if (dto == null) return BadRequest("Template data is required");
        var created = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { name = created.Name, version = created.Version }, created);
    }

    [HttpPut("{name}/{version:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<QuestionnaireTemplateDto>> Update(string name, int version, [FromBody] QuestionnaireTemplateDto dto, CancellationToken cancellationToken)
    {
        if (dto == null) return BadRequest("Template data is required");
        dto.Name = name;
        dto.Version = version;

        _logger.LogInformation(
            "QuestionnaireTemplate update started. TraceId={TraceId} Name={Name} Version={Version} Sections={SectionsCount}",
            HttpContext.TraceIdentifier,
            name,
            version,
            dto.Sections?.Count ?? 0);

        var updated = await _service.UpdateAsync(dto, cancellationToken);

        _logger.LogInformation(
            "QuestionnaireTemplate update succeeded. TraceId={TraceId} Name={Name} Version={Version}",
            HttpContext.TraceIdentifier,
            name,
            version);

        return Ok(updated);
    }

    [HttpDelete("{name}/{version:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<QuestionnaireTemplateDeleteResultDto>> Delete(string name, int version, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(name, version, cancellationToken);
        if (!result.IsSuccess)
            return result.Status == ResultStatus.NotFound ? NotFound() : BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    [HttpPost("{name}/{version:int}/publish")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> Publish(string name, int version, CancellationToken cancellationToken)
    {
        var result = await _service.PublishAsync(name, version, cancellationToken);
        if (!result.IsSuccess)
            return result.Status == ResultStatus.NotFound ? NotFound() : BadRequest(result.ValidationErrors);
        return NoContent();
    }

    [HttpPost("{name}/{version:int}/restore")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> Restore(string name, int version, CancellationToken cancellationToken)
    {
        var result = await _service.RestoreAsync(name, version, cancellationToken);
        if (!result.IsSuccess)
            return result.Status == ResultStatus.NotFound ? NotFound() : BadRequest(result.ValidationErrors);
        return NoContent();
    }

    [HttpPost("{name}/{version:int}/duplicate")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<QuestionnaireTemplateDto>> Duplicate(string name, int version, [FromBody] DuplicateTemplateRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest("Duplicate request is required");
        var duplicated = await _service.DuplicateTemplateAsync(name, version, request, cancellationToken);
        return Ok(duplicated);
    }

    [HttpGet("{name}/history/{entityType}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<VersionHistoryItemDto>>> GetVersionHistory(string name, string entityType, CancellationToken cancellationToken)
    {
        var history = await _service.GetVersionHistoryAsync(name, entityType, cancellationToken);
        return Ok(history);
    }

    [HttpPost("{name}/{version:int}/sections/{sectionOrder:int}/questions/{questionName}/active/{questionVersion:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<QuestionnaireTemplateDto>> SetActiveQuestionVersion(
        string name,
        int version,
        int sectionOrder,
        string questionName,
        int questionVersion,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetActiveQuestionVersionAsync(
            name,
            version,
            sectionOrder,
            questionName,
            questionVersion,
            cancellationToken);

        return Ok(updated);
    }

    [HttpGet("questions/{questionName}/{questionVersion:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<QuestionnaireQuestionDto>> GetQuestionVersion(
        string questionName,
        int questionVersion,
        CancellationToken cancellationToken)
    {
        var dto = await _service.GetQuestionVersionAsync(questionName, questionVersion, cancellationToken);
        return Ok(dto);
    }

    [HttpGet("questions/{questionName}/history")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<QuestionnaireQuestionHistoryDetailsDto>>> GetQuestionHistoryWithOptions(
        string questionName,
        CancellationToken cancellationToken)
    {
        var dtos = await _service.GetQuestionHistoryWithOptionsAsync(questionName, cancellationToken);
        return Ok(dtos);
    }

    [HttpPost("import/validate")]
    [Authorize(Policy = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_485_760)] // 10MB
    public async Task<ActionResult<QuestionnaireTemplateImportValidationResultDto>> ValidateImport(
        IFormFile file,
        [FromForm] string? scope,
        [FromForm] string? templateName,
        [FromForm] int? templateVersion,
        [FromForm] string? templateType,
        [FromForm] int? targetSectionOrder,
        [FromForm] string? title,
        [FromForm] string? description,
        [FromForm] int? timeLimitSeconds,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        var request = new QuestionnaireTemplateImportRequestDto
        {
            Scope = Enum.TryParse<QuestionnaireTemplateImportScope>(scope, true, out var parsedScope) ? parsedScope : null,
            TemplateName = templateName,
            TemplateVersion = templateVersion,
            TemplateType = templateType,
            TargetSectionOrder = targetSectionOrder,
            Title = title,
            Description = description,
            TimeLimitSeconds = timeLimitSeconds
        };

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;
        var result = await _importService.ValidateAsync(stream, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("import/execute")]
    [Authorize(Policy = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_485_760)] // 10MB
    public async Task<ActionResult<QuestionnaireTemplateImportExecuteResultDto>> ExecuteImport(
        IFormFile file,
        [FromForm] string? scope,
        [FromForm] string? templateName,
        [FromForm] int? templateVersion,
        [FromForm] string? templateType,
        [FromForm] int? targetSectionOrder,
        [FromForm] string? title,
        [FromForm] string? description,
        [FromForm] int? timeLimitSeconds,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        var request = new QuestionnaireTemplateImportRequestDto
        {
            Scope = Enum.TryParse<QuestionnaireTemplateImportScope>(scope, true, out var parsedScope) ? parsedScope : null,
            TemplateName = templateName,
            TemplateVersion = templateVersion,
            TemplateType = templateType,
            TargetSectionOrder = targetSectionOrder,
            Title = title,
            Description = description,
            TimeLimitSeconds = timeLimitSeconds
        };

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;
        var result = await _importService.ExecuteAsync(stream, request, cancellationToken);
        return Ok(result);
    }
}


