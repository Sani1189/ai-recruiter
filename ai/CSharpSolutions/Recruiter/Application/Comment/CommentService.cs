using Ardalis.Result;
using AutoMapper;
using Recruiter.Application.Comment.Dto;
using Recruiter.Application.Comment.Interfaces;
using Recruiter.Application.Comment.Queries;
using Recruiter.Application.Comment.Specifications;
using Recruiter.Application.Common.Dto;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.Comment;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _repository;
    private readonly IMapper _mapper;
    private readonly CommentQueryHandler _queryHandler;

    public CommentService(ICommentRepository repository, IMapper mapper, CommentQueryHandler queryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    public async Task<Result<CommentDto>> CreateAsync(CommentDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            return Result<CommentDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(dto), ErrorMessage = "Comment data is required." }
            });
        }

        // Validate parent comment exists if provided
        if (dto.ParentCommentId.HasValue)
        {
            var parentExistsResult = await _queryHandler.ExistsAsync(dto.ParentCommentId.Value, cancellationToken);
            if (!parentExistsResult.IsSuccess || !parentExistsResult.Value)
            {
                return Result<CommentDto>.NotFound($"Parent comment with ID {dto.ParentCommentId.Value} not found.");
            }
        }

        // Check if comment exists for this entity (only one comment per candidate)
        // Only check for top-level comments (no parent)
        if (!dto.ParentCommentId.HasValue)
        {
            var existingCommentSpec = new CommentByEntitySpecification(dto.EntityType, dto.EntityId);
            var existingComment = await _repository.FirstOrDefaultAsync(existingCommentSpec, cancellationToken);
            
            if (existingComment != null)
            {
                // Update existing comment instead of creating new one
                existingComment.Content = dto.Content;
                await _repository.UpdateAsync(existingComment, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                var updatedDto = _mapper.Map<CommentDto>(existingComment);
                return Result<CommentDto>.Success(updatedDto);
            }
        }

        // Create new comment if none exists
        var entity = _mapper.Map<Domain.Models.Comment>(dto);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var createdDto = _mapper.Map<CommentDto>(entity);
        return Result<CommentDto>.Success(createdDto);
    }

    public async Task<Result<CommentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<List<CommentDto>>> GetByEntityAsync(
        CommentableEntityType entityType,
        Guid entityId,
        bool includeReplies = false,
        CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByEntityAsync(entityType, entityId, includeReplies, cancellationToken);
    }

    public async Task<Result<List<CommentDto>>> GetThreadAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetThreadAsync(commentId, cancellationToken);
    }

    public async Task<Result<CommentDto>> UpdateAsync(CommentDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null || !dto.Id.HasValue)
        {
            return Result<CommentDto>.Invalid(new List<ValidationError>
            {
                new() { Identifier = nameof(dto), ErrorMessage = "Comment data with ID is required." }
            });
        }

        var spec = new CommentByIdSpecification(dto.Id.Value);
        var comment = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (comment == null)
        {
            return Result<CommentDto>.NotFound($"Comment with ID {dto.Id.Value} not found.");
        }

        // Only allow Content updates (EntityId, EntityType, ParentCommentId are immutable)
        comment.Content = dto.Content;
        
        await _repository.UpdateAsync(comment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<CommentDto>(comment);
        return Result<CommentDto>.Success(resultDto);
    }

    public async Task<Result<Common.Dto.PagedResult<CommentDto>>> GetFilteredCommentsAsync(
        Dto.CommentListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredCommentsAsync(query, cancellationToken);
    }
}

