using AutoMapper;
using Ardalis.Result;
using Recruiter.Application.Comment.Dto;
using Recruiter.Application.Comment.Interfaces;
using Recruiter.Application.Comment.Specifications;
using Recruiter.Application.Common.Dto;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.Comment.Queries;

public class CommentQueryHandler
{
    private readonly ICommentRepository _repository;
    private readonly IMapper _mapper;

    public CommentQueryHandler(ICommentRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Result<CommentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new CommentByIdSpecification(id);
            var comment = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

            if (comment == null)
            {
                return Result<CommentDto>.NotFound($"Comment with ID {id} not found.");
            }

            var dto = _mapper.Map<CommentDto>(comment);
            return Result<CommentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result<CommentDto>.Error();
        }
    }

    public async Task<Result<List<CommentDto>>> GetByEntityAsync(
        CommentableEntityType entityType,
        Guid entityId,
        bool includeReplies = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new CommentsByEntitySpecification(entityType, entityId, includeReplies);
            var comments = await _repository.ListAsync(spec, cancellationToken);
            var dtos = _mapper.Map<List<CommentDto>>(comments);
            return Result<List<CommentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result<List<CommentDto>>.Error();
        }
    }

    public async Task<Result<List<CommentDto>>> GetThreadAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        try
        {
            // First verify the parent comment exists
            var parentSpec = new CommentByIdSpecification(commentId);
            var parentExists = await _repository.AnyAsync(parentSpec, cancellationToken);
            if (!parentExists)
            {
                return Result<List<CommentDto>>.NotFound($"Comment with ID {commentId} not found.");
            }

            var threadSpec = new CommentThreadSpecification(commentId);
            var comments = await _repository.ListAsync(threadSpec, cancellationToken);
            var dtos = _mapper.Map<List<CommentDto>>(comments);
            return Result<List<CommentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result<List<CommentDto>>.Error();
        }
    }

    public async Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new CommentByIdSpecification(id);
            var exists = await _repository.AnyAsync(spec, cancellationToken);
            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result<bool>.Error();
        }
    }

    public async Task<Result<Common.Dto.PagedResult<CommentDto>>> GetFilteredCommentsAsync(
        CommentListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var countSpec = new CommentFilterCountSpec(query);
            var filterSpec = new CommentFilterSpec(query);

            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);
            var comments = await _repository.ListAsync(filterSpec, cancellationToken);
            var dtos = _mapper.Map<List<CommentDto>>(comments);

            var result = new Common.Dto.PagedResult<CommentDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<CommentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result<Common.Dto.PagedResult<CommentDto>>.Error();
        }
    }
}
