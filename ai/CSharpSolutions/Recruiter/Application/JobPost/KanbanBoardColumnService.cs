using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost;

public interface IKanbanBoardColumnService
{
    Task<IEnumerable<KanbanBoardColumnDto>> GetColumnsByRecruiterAsync(Guid recruiterId);
    Task<KanbanBoardColumnDto> CreateColumnAsync(Guid recruiterId, KanbanBoardColumnDto dto);
    Task<KanbanBoardColumnDto?> UpdateColumnAsync(Guid columnId, KanbanBoardColumnDto dto);
    Task<bool> DeleteColumnAsync(Guid columnId);
    Task<bool> ReorderColumnsAsync(Guid recruiterId, List<(Guid ColumnId, int Sequence)> ordering);
    Task<KanbanBoardColumnDto?> GetColumnByIdAsync(Guid columnId);
}

public class KanbanBoardColumnService : IKanbanBoardColumnService
{
    private readonly IRepository<KanbanBoardColumn> _repository;
    private readonly IMapper _mapper;

    public KanbanBoardColumnService(
        IRepository<KanbanBoardColumn> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<KanbanBoardColumnDto>> GetColumnsByRecruiterAsync(Guid recruiterId)
    {
        var columns = await _repository.ListAsync(c => c.RecruiterId == recruiterId);
        var ordered = columns.OrderBy(c => c.Sequence).ToList();
        return _mapper.Map<IEnumerable<KanbanBoardColumnDto>>(ordered);
    }

    public async Task<KanbanBoardColumnDto> CreateColumnAsync(Guid recruiterId, KanbanBoardColumnDto dto)
    {
        // Get the highest sequence number and add 1
        var existingColumns = await _repository.ListAsync(c => c.RecruiterId == recruiterId);
        var maxSequence = existingColumns.Any() ? existingColumns.Max(c => c.Sequence) : 0;

        var newColumn = new KanbanBoardColumn
        {
            Id = Guid.NewGuid(),
            RecruiterId = recruiterId,
            ColumnName = dto.ColumnName,
            Sequence = maxSequence + 1,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(newColumn);
        return _mapper.Map<KanbanBoardColumnDto>(newColumn);
    }

    public async Task<KanbanBoardColumnDto?> UpdateColumnAsync(Guid columnId, KanbanBoardColumnDto dto)
    {
        var column = await _repository.GetByIdAsync(columnId);
        if (column == null)
            return null;

        column.ColumnName = dto.ColumnName;
        column.IsVisible = dto.IsVisible;
        column.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(column);
        return _mapper.Map<KanbanBoardColumnDto>(column);
    }

    public async Task<bool> DeleteColumnAsync(Guid columnId)
    {
        var column = await _repository.GetByIdAsync(columnId);
        if (column == null)
            return false;

        await _repository.DeleteAsync(column);
        return true;
    }

    public async Task<bool> ReorderColumnsAsync(Guid recruiterId, List<(Guid ColumnId, int Sequence)> ordering)
    {
        var columns = await _repository.ListAsync(c => c.RecruiterId == recruiterId);
        
        foreach (var (columnId, sequence) in ordering)
        {
            var column = columns.FirstOrDefault(c => c.Id == columnId);
            if (column != null)
            {
                column.Sequence = sequence;
                column.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(column);
            }
        }

        return true;
    }

    public async Task<KanbanBoardColumnDto?> GetColumnByIdAsync(Guid columnId)
    {
        var column = await _repository.GetByIdAsync(columnId);
        return column != null ? _mapper.Map<KanbanBoardColumnDto>(column) : null;
    }
}
