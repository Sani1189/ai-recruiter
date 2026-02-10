using Ardalis.Result;
using Recruiter.Application.Comment.Dto;
using Recruiter.Application.Common.Dto;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.Comment.Interfaces;

public interface ICommentService
{
    Task<Result<CommentDto>> CreateAsync(CommentDto dto, CancellationToken cancellationToken = default);
    Task<Result<CommentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<CommentDto>>> GetByEntityAsync(
        CommentableEntityType entityType,
        Guid entityId,
        bool includeReplies = false,
        CancellationToken cancellationToken = default);
    Task<Result<List<CommentDto>>> GetThreadAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<Result<CommentDto>> UpdateAsync(CommentDto dto, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<CommentDto>>> GetFilteredCommentsAsync(Dto.CommentListQueryDto query, CancellationToken cancellationToken = default);
}

