using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.JobPost;
using Recruiter.Application.JobPost.Dto;

namespace Recruiter.WebApi.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Admin")]
public class KanbanBoardColumnController(
    IKanbanBoardColumnService kanbanService) : ControllerBase
{
    private readonly IKanbanBoardColumnService _kanbanService = kanbanService;

    /// <summary>
    /// Get all kanban board columns for a recruiter
    /// </summary>
    [HttpGet("recruiter/{recruiterId}")]
    public async Task<ActionResult<IEnumerable<KanbanBoardColumnDto>>> GetColumnsByRecruiter(Guid recruiterId)
    {
        var columns = await _kanbanService.GetColumnsByRecruiterAsync(recruiterId);
        return Ok(columns);
    }

    /// <summary>
    /// Get a specific kanban board column
    /// </summary>
    [HttpGet("{columnId}")]
    public async Task<ActionResult<KanbanBoardColumnDto>> GetColumn(Guid columnId)
    {
        var column = await _kanbanService.GetColumnByIdAsync(columnId);
        if (column == null)
            return NotFound();

        return Ok(column);
    }

    /// <summary>
    /// Create a new kanban board column for a recruiter
    /// </summary>
    [HttpPost("recruiter/{recruiterId}")]
    public async Task<ActionResult<KanbanBoardColumnDto>> CreateColumn(Guid recruiterId, [FromBody] KanbanBoardColumnDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        dto.RecruiterId = recruiterId;
        var column = await _kanbanService.CreateColumnAsync(recruiterId, dto);
        return CreatedAtAction(nameof(GetColumn), new { columnId = column.Id }, column);
    }

    /// <summary>
    /// Update a kanban board column
    /// </summary>
    [HttpPut("{columnId}")]
    public async Task<ActionResult<KanbanBoardColumnDto>> UpdateColumn(Guid columnId, [FromBody] KanbanBoardColumnDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updatedColumn = await _kanbanService.UpdateColumnAsync(columnId, dto);
        if (updatedColumn == null)
            return NotFound();

        return Ok(updatedColumn);
    }

    /// <summary>
    /// Delete a kanban board column
    /// </summary>
    [HttpDelete("{columnId}")]
    public async Task<ActionResult> DeleteColumn(Guid columnId)
    {
        var success = await _kanbanService.DeleteColumnAsync(columnId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Reorder kanban board columns for a recruiter
    /// </summary>
    [HttpPost("recruiter/{recruiterId}/reorder")]
    public async Task<ActionResult> ReorderColumns(Guid recruiterId, [FromBody] List<KanbanBoardColumnOrderDto> ordering)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var orderingList = ordering.Select(o => (o.ColumnId, o.Sequence)).ToList();
        var success = await _kanbanService.ReorderColumnsAsync(recruiterId, orderingList);

        if (!success)
            return BadRequest("Failed to reorder columns");

        return Ok();
    }
}

/// <summary>
/// DTO for kanban board column ordering
/// </summary>
public class KanbanBoardColumnOrderDto
{
    public Guid ColumnId { get; set; }
    public int Sequence { get; set; }
}
