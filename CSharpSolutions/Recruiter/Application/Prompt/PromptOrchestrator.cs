using Ardalis.Result;
using FluentValidation;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Application.Prompt.Interfaces;

namespace Recruiter.Application.Prompt;

// Prompt Orchestrator for complex business operations
public interface IPromptOrchestrator
{
    Task<Result<PromptDto>> GetPromptWithVariablesAsync(string name, int version, CancellationToken cancellationToken = default);
    Task<Result<PromptDto>> ProcessPromptAsync(PromptDto promptDto, CancellationToken cancellationToken = default);
    Task<Result<PromptDto>> ActivatePromptAsync(string name, int version, CancellationToken cancellationToken = default);
}

// Prompt Orchestrator implementation
public class PromptOrchestrator : IPromptOrchestrator
{
    private readonly IPromptService _promptService;
    private readonly IValidator<PromptDto> _promptValidator;

    public PromptOrchestrator(
        IPromptService promptService,
        IValidator<PromptDto> promptValidator)
    {
        _promptService = promptService;
        _promptValidator = promptValidator;
    }

    public async Task<Result<PromptDto>> GetPromptWithVariablesAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        var prompt = await _promptService.GetByIdAsync(name, version);
        if (prompt == null)
        {
            return Result<PromptDto>.NotFound($"Prompt with name {name} and version {version} not found");
        }

        // Process variables would be handled here
        // This is a simple implementation - in real scenario you might want to include processed content

        return Result<PromptDto>.Success(prompt);
    }

    public async Task<Result<PromptDto>> ProcessPromptAsync(PromptDto promptDto, CancellationToken cancellationToken = default)
    {
        // Validate the prompt
        var validationResult = await _promptValidator.ValidateAsync(promptDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PromptDto>.Invalid(validationResult.Errors.Select(e => new ValidationError { ErrorMessage = e.ErrorMessage }).ToArray());
        }

        // Process the prompt (create or update)
        PromptDto result;
        if (string.IsNullOrEmpty(promptDto.Name))
        {
            result = await _promptService.CreateAsync(promptDto);
        }
        else
        {
            result = await _promptService.UpdateAsync(promptDto);
        }

        return Result<PromptDto>.Success(result);
    }

    public async Task<Result<PromptDto>> ActivatePromptAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        var prompt = await _promptService.GetByIdAsync(name, version);
        if (prompt == null)
        {
            return Result<PromptDto>.NotFound($"Prompt with name {name} and version {version} not found");
        }

        // Activate prompt (if there was an Active property)
        // prompt.Active = true;
        var updatedPrompt = await _promptService.UpdateAsync(prompt);

        return Result<PromptDto>.Success(updatedPrompt);
    }
}
