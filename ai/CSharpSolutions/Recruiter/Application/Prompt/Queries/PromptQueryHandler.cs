using Ardalis.Result;
using AutoMapper;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Application.Prompt.Interfaces;
using Recruiter.Application.Prompt.Specifications;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Dto;
using System.Reflection.Metadata.Ecma335;

namespace Recruiter.Application.Prompt.Queries;

// Prompt query handler using Ardalis specification pattern
public class PromptQueryHandler
{
    private readonly IPromptRepository _promptRepository;
    private readonly IMapper _mapper;

    public PromptQueryHandler(IPromptRepository promptRepository, IMapper mapper)
    {
        _promptRepository = promptRepository;
        _mapper = mapper;
    }

    // Get prompt by name and version using specification pattern
    public async Task<Result<PromptDto>> GetByNameAndVersionAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<PromptDto>.Invalid(new ValidationError { ErrorMessage = "Invalid prompt name" });

        var spec = new PromptByNameAndVersionSpec(name, version);
        var prompt = await _promptRepository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (prompt == null)
            return Result<PromptDto>.NotFound($"Prompt with name {name} and version {version} not found");

        var promptDto = _mapper.Map<PromptDto>(prompt);
        return Result<PromptDto>.Success(promptDto);
    }

    // Get latest prompt by name using specification pattern
    public async Task<Result<PromptDto>> GetLatestByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<PromptDto>.Invalid(new ValidationError { ErrorMessage = "Invalid prompt name" });

        var spec = new PromptLatestByNameSpec(name);
        var prompt = await _promptRepository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (prompt == null)
            return Result<PromptDto>.NotFound($"Prompt with name {name} not found");

        var promptDto = _mapper.Map<PromptDto>(prompt);
        return Result<PromptDto>.Success(promptDto);
    }

    // Get prompts by category using specification pattern
    public async Task<Result<List<PromptDto>>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
            return Result<List<PromptDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid prompt category" });

        var spec = new PromptsByCategorySpec(category);
        var prompts = await _promptRepository.ListAsync(spec, cancellationToken);
        var promptDtos = _mapper.Map<List<PromptDto>>(prompts);
        
        return Result<List<PromptDto>>.Success(promptDtos);
    }

    // Get prompts by locale using specification pattern
    public async Task<Result<List<PromptDto>>> GetByLocaleAsync(string locale, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(locale))
            return Result<List<PromptDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid locale" });

        var spec = new PromptsByLocaleSpec(locale);
        var prompts = await _promptRepository.ListAsync(spec, cancellationToken);
        var promptDtos = _mapper.Map<List<PromptDto>>(prompts);
        
        return Result<List<PromptDto>>.Success(promptDtos);
    }

    // Get filtered prompts with pagination
    public async Task<Result<Common.Dto.PagedResult<PromptDto>>> GetFilteredPromptsAsync(PromptListQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            var countSpec = new PromptFilterCountSpec(query);
            var filterSpec = new PromptFilterSpec(query);

            var totalCount = await _promptRepository.CountAsync(countSpec, cancellationToken);
            var prompts = await _promptRepository.ListAsync(filterSpec, cancellationToken);

            var promptDtos = _mapper.Map<List<PromptDto>>(prompts);

            var result = new Common.Dto.PagedResult<PromptDto>
            {
                Items = promptDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<PromptDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<PromptDto>>.Error();
        }
    }

    public async Task<Result<List<PromptDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var spec = new ExcludeSoftDeletedPromptsSpec();
        var prompts = await _promptRepository.ListAsync(spec, cancellationToken);
        var promptDtos = _mapper.Map<List<PromptDto>>(prompts);
        
        return Result<List<PromptDto>>.Success(promptDtos);
    }

    public async Task<Result<List<string>>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _promptRepository.GetDistinctCategoriesAsync(cancellationToken);
            return Result<List<string>>.Success(categories);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<List<string>>.Error();
        }
    }
}
