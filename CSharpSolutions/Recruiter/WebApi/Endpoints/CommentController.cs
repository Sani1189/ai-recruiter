using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.Comment.Dto;
using Recruiter.Application.Comment.Interfaces;
using Recruiter.Application.Common.Dto;
using Recruiter.Domain.Enums;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentController(ICommentService commentService) : ControllerBase
{
    private readonly ICommentService _commentService = commentService;

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CommentDto dto, CancellationToken cancellationToken)
    {
        var result = await _commentService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetCommentById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetCommentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commentService.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<List<CommentDto>>> GetCommentsByEntity(
        CommentableEntityType entityType,
        Guid entityId,
        [FromQuery] bool includeReplies = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _commentService.GetByEntityAsync(entityType, entityId, includeReplies, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(result.Value);
    }

    [HttpGet("{id}/thread")]
    public async Task<ActionResult<List<CommentDto>>> GetCommentThread(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commentService.GetThreadAsync(id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CommentDto>> UpdateComment(
        Guid id,
        [FromBody] CommentDto dto,
        CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest("Comment data is required");

        dto.Id = id;
        var result = await _commentService.UpdateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<CommentDto>>> GetFilteredComments([FromQuery] CommentListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _commentService.GetFilteredCommentsAsync(query, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Errors);

        return Ok(result.Value);
    }
}
