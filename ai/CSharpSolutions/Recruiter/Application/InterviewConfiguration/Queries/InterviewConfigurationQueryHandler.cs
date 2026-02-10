using Ardalis.Result;
using AutoMapper;
using Recruiter.Application.InterviewConfiguration.Dto;
using Recruiter.Application.InterviewConfiguration.Specifications;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.InterviewConfiguration.Queries;

// InterviewConfiguration query handler using Ardalis specification pattern
public class InterviewConfigurationQueryHandler
{
    private readonly IRepository<Domain.Models.InterviewConfiguration> _repository;
    private readonly IMapper _mapper;

    public InterviewConfigurationQueryHandler(IRepository<Domain.Models.InterviewConfiguration> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Get interview configuration by name and version using specification pattern
    public async Task<Result<InterviewConfigurationDto>> GetByNameAndVersionAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<InterviewConfigurationDto>.Invalid(new ValidationError { ErrorMessage = "Invalid configuration name" });

        var spec = new InterviewConfigurationByNameAndVersionSpec(name, version);
        var configuration = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (configuration == null)
            return Result<InterviewConfigurationDto>.NotFound($"Interview configuration with name {name} and version {version} not found");

        var configurationDto = _mapper.Map<InterviewConfigurationDto>(configuration);
        return Result<InterviewConfigurationDto>.Success(configurationDto);
    }

    // Get latest interview configuration by name using specification pattern
    public async Task<Result<InterviewConfigurationDto>> GetLatestByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<InterviewConfigurationDto>.Invalid(new ValidationError { ErrorMessage = "Invalid configuration name" });

        var spec = new InterviewConfigurationLatestByNameSpec(name);
        var configuration = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (configuration == null)
            return Result<InterviewConfigurationDto>.NotFound($"Interview configuration with name {name} not found");

        var configurationDto = _mapper.Map<InterviewConfigurationDto>(configuration);
        return Result<InterviewConfigurationDto>.Success(configurationDto);
    }

    // Get interview configurations by modality using specification pattern
    public async Task<Result<List<InterviewConfigurationDto>>> GetByModalityAsync(string modality, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modality))
            return Result<List<InterviewConfigurationDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid modality" });

        var spec = new InterviewConfigurationsByModalitySpec(modality);
        var configurations = await _repository.ListAsync(spec, cancellationToken);
        var configurationDtos = _mapper.Map<List<InterviewConfigurationDto>>(configurations);
        
        return Result<List<InterviewConfigurationDto>>.Success(configurationDtos);
    }

    // Get active interview configurations using specification pattern
    public async Task<Result<List<InterviewConfigurationDto>>> GetActiveConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        var spec = new ActiveInterviewConfigurationsSpec();
        var configurations = await _repository.ListAsync(spec, cancellationToken);
        var configurationDtos = _mapper.Map<List<InterviewConfigurationDto>>(configurations);
        
        return Result<List<InterviewConfigurationDto>>.Success(configurationDtos);
    }

    // Get filtered interview configurations with pagination
    public async Task<Result<Common.Dto.PagedResult<InterviewConfigurationDto>>> GetFilteredConfigurationsAsync(InterviewConfigurationListQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            var countSpec = new InterviewConfigurationFilterCountSpec(query);
            var filterSpec = new InterviewConfigurationFilterSpec(query);

            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);
            var configurations = await _repository.ListAsync(filterSpec, cancellationToken);

            var configurationDtos = _mapper.Map<List<InterviewConfigurationDto>>(configurations);

            var result = new Common.Dto.PagedResult<InterviewConfigurationDto>
            {
                Items = configurationDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<InterviewConfigurationDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<InterviewConfigurationDto>>.Error();
        }
    }
}
