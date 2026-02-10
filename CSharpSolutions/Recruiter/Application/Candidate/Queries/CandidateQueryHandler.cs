using AutoMapper;
using Ardalis.Result;
using Recruiter.Application.Candidate.Dto;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Application.Candidate.Specifications;
using Recruiter.Application.Comment.Dto;
using Recruiter.Application.Comment.Interfaces;
using Recruiter.Application.Comment.Specifications;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.CvEvaluation.Specifications;
using Recruiter.Application.KeyStrength.Specifications;
using Recruiter.Application.Scoring.Specifications;
using Recruiter.Application.Summary.Specifications;
using Recruiter.Application.KeyStrength.Dto;
using Recruiter.Application.Scoring.Dto;
using Recruiter.Application.Summary.Dto;

namespace Recruiter.Application.Candidate.Queries;

/// Handles complex candidate queries with pagination and filtering
public class CandidateQueryHandler
{
    private readonly ICandidateRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICommentRepository _commentRepository;
    private readonly IRepository<Domain.Models.KeyStrength> _keyStrengthRepository;
    private readonly IRepository<Domain.Models.Summary> _summaryRepository;
    private readonly IRepository<Domain.Models.Scoring> _scoringRepository;
    private readonly IRepository<Domain.Models.CvEvaluation> _cvEvaluationRepository;


    public CandidateQueryHandler(
        ICandidateRepository repository,
        ICommentRepository commentRepository,
        IRepository<Domain.Models.KeyStrength> keyStrengthRepository,
        IRepository<Domain.Models.Summary> summaryRepository,
        IRepository<Domain.Models.Scoring> scoringRepository,
        IRepository<Domain.Models.CvEvaluation> cvEvaluationRepository,
        IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
        _keyStrengthRepository = keyStrengthRepository ?? throw new ArgumentNullException(nameof(keyStrengthRepository));
        _summaryRepository = summaryRepository ?? throw new ArgumentNullException(nameof(summaryRepository));
        _scoringRepository = scoringRepository ?? throw new ArgumentNullException(nameof(scoringRepository));
        _cvEvaluationRepository = cvEvaluationRepository ?? throw new ArgumentNullException(nameof(cvEvaluationRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        
    }

    // Get candidates by user ID
    public async Task<Result<List<CandidateDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new CandidateByUserIdSpec(userId);
            var candidates = await _repository.ListAsync(spec, cancellationToken);
            var candidateDtos = _mapper.Map<List<CandidateDto>>(candidates);
            
            await EnrichScoringsAsync(candidates, candidateDtos, cancellationToken);
            
            return Result<List<CandidateDto>>.Success(candidateDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<CandidateDto>>.Error();
        }
    }

    /// <summary>
    /// Enrich DTOs with latest evaluation and scorings using batch queries for efficiency.
    /// </summary>
    private async Task EnrichScoringsAsync(
        IReadOnlyList<Domain.Models.Candidate> entities,
        IReadOnlyList<CandidateDto> dtos,
        CancellationToken cancellationToken)
    {
        if (!entities.Any() || !dtos.Any())
            return;

        // Extract unique user profile IDs
        var userProfileIds = entities.Select(e => e.UserId).Distinct().ToList();

        // Batch fetch all latest CvEvaluations for all user profiles
        var allEvaluations = await _cvEvaluationRepository
            .ListAsync(new CvEvaluationsLatestByUserProfileIdsSpec(userProfileIds), cancellationToken);

        // Group by UserProfileId and take the latest for each (by CreatedAt, then Id)
        var latestEvaluationsByUserProfile = allEvaluations
            .GroupBy(e => e.UserProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(e => e.CreatedAt)
                      .ThenByDescending(e => e.Id)
                      .First()
            );

        // Collect CvEvaluationIds that have evaluations
        var cvEvaluationIds = latestEvaluationsByUserProfile.Values.Select(e => e.Id).ToList();

        // Batch fetch all scorings for evaluations
        var scoringsByEvaluationId = new Dictionary<Guid, List<Domain.Models.Scoring>>();
        if (cvEvaluationIds.Any())
        {
            var allScorings = await _scoringRepository
                .ListAsync(new ScoringsByCvEvaluationIdsSpec(cvEvaluationIds), cancellationToken);

            scoringsByEvaluationId = allScorings
                .GroupBy(s => s.CvEvaluationId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // Find user profiles without evaluations for fallback scoring lookup
        var userProfileIdsWithoutEvaluation = userProfileIds
            .Where(id => !latestEvaluationsByUserProfile.ContainsKey(id))
            .ToList();

        // Batch fetch fallback scorings for user profiles without evaluations
        var fallbackScoringsByUserProfile = new Dictionary<Guid, List<Domain.Models.Scoring>>();
        if (userProfileIdsWithoutEvaluation.Any())
        {
            var fallbackScorings = await _scoringRepository
                .ListAsync(new ScoringsByUserProfileIdsSpec(userProfileIdsWithoutEvaluation), cancellationToken);

            fallbackScoringsByUserProfile = fallbackScorings
                .GroupBy(s => s.CvEvaluation!.UserProfileId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // Map scorings to DTOs
        foreach (var (entity, dto) in entities.Zip(dtos, (e, d) => (e, d)))
        {
            var userProfileId = entity.UserId;

            if (latestEvaluationsByUserProfile.TryGetValue(userProfileId, out var latestEvaluation))
            {
                dto.LatestCvEvaluationId = latestEvaluation.Id;

                if (scoringsByEvaluationId.TryGetValue(latestEvaluation.Id, out var scorings))
                {
                    dto.Scorings = _mapper.Map<List<ScoringDto>>(scorings);
                }
            }
            else if (fallbackScoringsByUserProfile.TryGetValue(userProfileId, out var fallbackScorings))
            {
                dto.Scorings = _mapper.Map<List<ScoringDto>>(fallbackScorings);
            }
        }
    }

    // Get candidates by CV file ID
    public async Task<Result<List<CandidateDto>>> GetByCvFileIdAsync(Guid cvFileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new CandidateByCvFileIdSpec(cvFileId);
            var candidates = await _repository.ListAsync(spec, cancellationToken);
            var candidateDtos = _mapper.Map<List<CandidateDto>>(candidates);
            return Result<List<CandidateDto>>.Success(candidateDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<CandidateDto>>.Error();
        }
    }

    // Get recent candidates
    public async Task<Result<List<CandidateDto>>> GetRecentCandidatesAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var spec = new CandidateFilterSpec(new CandidateListQueryDto 
            { 
                CreatedAfter = cutoffDate,
                PageNumber = 1,
                PageSize = 100
            });
            var candidates = await _repository.ListAsync(spec, cancellationToken);
            var candidateDtos = _mapper.Map<List<CandidateDto>>(candidates);
            
            await EnrichScoringsAsync(candidates, candidateDtos, cancellationToken);
            
            return Result<List<CandidateDto>>.Success(candidateDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<CandidateDto>>.Error();
        }
    }

    // Advanced query with filtering, sorting, and pagination
    public async Task<Result<Common.Dto.PagedResult<CandidateDto>>> GetFilteredCandidatesAsync(CandidateListQueryDto query, CancellationToken cancellationToken = default)
    {
        // Validate query parameters
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            // Use specifications for complex queries
            var countSpec = new CandidateFilterCountSpec(query);
            var filterSpec = new CandidateFilterSpec(query);

            // Get total count efficiently
            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

            // Get filtered and paged results
            var candidates = await _repository.ListAsync(filterSpec, cancellationToken);

            // Map to DTOs
            var candidateDtos = _mapper.Map<List<CandidateDto>>(candidates);

            await EnrichScoringsAsync(candidates, candidateDtos, cancellationToken);

            var result = new Common.Dto.PagedResult<CandidateDto>
            {
                Items = candidateDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<CandidateDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<CandidateDto>>.Error();
        }
    }

    // Enhanced methods with UserProfile information
    public async Task<Result<CandidateDto>> GetByUserIdWithUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var spec = new CandidateByUserIdSpec(userId);
            var candidates = await _repository.ListAsync(spec, cancellationToken);
            var candidate = candidates.FirstOrDefault();
            
            if (candidate == null)
                return Result<CandidateDto>.NotFound();

            var candidateDto = _mapper.Map<CandidateDto>(candidate);
            var candidateList = new List<Domain.Models.Candidate> { candidate };
            var dtoList = new List<CandidateDto> { candidateDto };
            
            await EnrichScoringsAsync(candidateList, dtoList, cancellationToken);
            
            return Result<CandidateDto>.Success(candidateDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<CandidateDto>.Error();
        }
    }

    public async Task<Result<List<CandidateDto>>> GetRecentCandidatesWithUserProfileAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var spec = new CandidateFilterSpec(new CandidateListQueryDto 
            { 
                CreatedAfter = cutoffDate,
                PageNumber = 1,
                PageSize = 100
            });
            var candidates = await _repository.ListAsync(spec, cancellationToken);
            var candidateDtos = _mapper.Map<List<CandidateDto>>(candidates);
            
            await EnrichScoringsAsync(candidates, candidateDtos, cancellationToken);
            
            return Result<List<CandidateDto>>.Success(candidateDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<CandidateDto>>.Error();
        }
    }

    public async Task<Result<Common.Dto.PagedResult<CandidateDto>>> GetFilteredCandidatesWithUserProfileAsync(CandidateListQueryDto query, CancellationToken cancellationToken = default)
    {
        // Validate query parameters
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            // Use specifications for complex queries
            var countSpec = new CandidateFilterCountSpec(query);
            var filterSpec = new CandidateFilterSpec(query);

            // Get total count efficiently
            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

            // Get filtered and paged results
            var candidates = await _repository.ListAsync(filterSpec, cancellationToken);

            // Map to DTOs
            var candidateDtos = _mapper.Map<List<CandidateDto>>(candidates);

            await EnrichScoringsAsync(candidates, candidateDtos, cancellationToken);

            var result = new Common.Dto.PagedResult<CandidateDto>
            {
                Items = candidateDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<CandidateDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<CandidateDto>>.Error();
        }
    }

    // Get candidate details with related entities by ID
    public async Task<Result<CandidateDto>> GetCandidateDetailsById(Guid id, CancellationToken cancellationToken = default)
    {
       try
        {
            var spec = new CandidateByIdSpec(id);
            var candidate = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
            
            if (candidate == null)
                return Result<CandidateDto>.NotFound();

            var candidateDto = _mapper.Map<CandidateDto>(candidate);

            // hydrate related comment (if any)
            // find the comment for this candidate
            var commentSpec = new CommentByEntitySpecification(Domain.Enums.CommentableEntityType.Candidate, id);
            var comment = await _commentRepository.FirstOrDefaultAsync(commentSpec, cancellationToken);
            if (comment != null)
            {
                candidateDto.Comment = _mapper.Map<CommentDto>(comment);
            }

            // ensure we have user profile id to gather derived data
            var userProfileId = candidate.UserProfile?.Id;
            if (userProfileId.HasValue)
            {
                // fetch latest CV evaluation for this user to attach scorings
                var latestEvaluation = await _cvEvaluationRepository
                    .FirstOrDefaultAsync(new CvEvaluationLatestByUserProfileSpec(userProfileId.Value), cancellationToken);

                if (latestEvaluation != null)
                {
                    candidateDto.LatestCvEvaluationId = latestEvaluation.Id;

                    var scorings = await _scoringRepository
                        .ListAsync(new ScoringByCvEvaluationIdSpec(latestEvaluation.Id), cancellationToken);
                    candidateDto.Scorings = _mapper.Map<List<ScoringDto>>(scorings);
                }

                // Key strengths for the profile
                var strengths = await _keyStrengthRepository
                    .ListAsync(new KeyStrengthByUserProfileSpec(userProfileId.Value), cancellationToken);
                candidateDto.KeyStrengths = _mapper.Map<List<KeyStrengthDto>>(strengths);

                // Summaries: take latest per type to avoid duplicates
                var summaries = await _summaryRepository
                    .ListAsync(new SummaryByUserProfileSpec(userProfileId.Value), cancellationToken);
                var latestSummaries = summaries
                    .GroupBy(s => s.Type, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(x => x.CreatedAt).First())
                    .ToList();
                candidateDto.Summaries = _mapper.Map<List<SummaryDto>>(latestSummaries);
            }

            return Result<CandidateDto>.Success(candidateDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<CandidateDto>.Error();
        }
    }
}
