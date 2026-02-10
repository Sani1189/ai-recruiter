using Ardalis.Result;
using FluentValidation;
using Recruiter.Application.InterviewConfiguration.Dto;
using Recruiter.Application.InterviewConfiguration.Interfaces;

namespace Recruiter.Application.InterviewConfiguration;

// InterviewConfiguration Orchestrator for complex business operations
public interface IInterviewConfigurationOrchestrator
{
    Task<Result<InterviewConfigurationDto>> GetConfigurationWithPromptsAsync(string name, int version, CancellationToken cancellationToken = default);
    Task<Result<InterviewConfigurationDto>> ProcessConfigurationAsync(InterviewConfigurationDto configurationDto, CancellationToken cancellationToken = default);
    Task<Result<InterviewConfigurationDto>> ActivateConfigurationAsync(string name, int version, CancellationToken cancellationToken = default);
}

// InterviewConfiguration Orchestrator implementation
public class InterviewConfigurationOrchestrator : IInterviewConfigurationOrchestrator
{
    private readonly IInterviewConfigurationService _configurationService;
    private readonly IValidator<InterviewConfigurationDto> _configurationValidator;

    public InterviewConfigurationOrchestrator(
        IInterviewConfigurationService configurationService,
        IValidator<InterviewConfigurationDto> configurationValidator)
    {
        _configurationService = configurationService;
        _configurationValidator = configurationValidator;
    }

    public async Task<Result<InterviewConfigurationDto>> GetConfigurationWithPromptsAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationService.GetByIdAsync(name, version);
        if (configuration == null)
        {
            return Result<InterviewConfigurationDto>.NotFound($"Interview configuration with name {name} and version {version} not found");
        }

        // Get related prompts would be handled here
        // This is a simple implementation - in real scenario you might want to include prompts in the DTO

        return Result<InterviewConfigurationDto>.Success(configuration);
    }

    public async Task<Result<InterviewConfigurationDto>> ProcessConfigurationAsync(InterviewConfigurationDto configurationDto, CancellationToken cancellationToken = default)
    {
        // Validate the configuration
        var validationResult = await _configurationValidator.ValidateAsync(configurationDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<InterviewConfigurationDto>.Invalid(validationResult.Errors.Select(e => new ValidationError { ErrorMessage = e.ErrorMessage }).ToArray());
        }

        // Process the configuration (create or update)
        InterviewConfigurationDto result;
        if (string.IsNullOrEmpty(configurationDto.Name))
        {
            result = await _configurationService.CreateAsync(configurationDto);
        }
        else
        {
            result = await _configurationService.UpdateAsync(configurationDto);
        }

        return Result<InterviewConfigurationDto>.Success(result);
    }

    public async Task<Result<InterviewConfigurationDto>> ActivateConfigurationAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationService.GetByIdAsync(name, version);
        if (configuration == null)
        {
            return Result<InterviewConfigurationDto>.NotFound($"Interview configuration with name {name} and version {version} not found");
        }

        // Activate configuration
        configuration.Active = true;
        var updatedConfiguration = await _configurationService.UpdateAsync(configuration);

        return Result<InterviewConfigurationDto>.Success(updatedConfiguration);
    }
}
