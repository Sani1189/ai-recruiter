using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.QuestionnaireTemplate;

/// <summary>
/// Handles retry logic for concurrent versioning operations with exponential backoff.
/// </summary>
public sealed class QuestionnaireTemplateRetryHandler : IQuestionnaireTemplateRetryHandler
{
    private readonly IRepository<DomainQuestionnaireTemplate> _templateRepository;
    private const int MaxRetryAttempts = 5;
    private const int BaseRetryDelayMs = 50;

    public QuestionnaireTemplateRetryHandler(IRepository<DomainQuestionnaireTemplate> templateRepository)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    public async Task<QuestionnaireTemplateDto> ExecuteWithRetryAsync(
        DomainQuestionnaireTemplate existing,
        Func<DomainQuestionnaireTemplate, CancellationToken, Task<QuestionnaireTemplateDto>> operation,
        CancellationToken cancellationToken = default)
    {
        var existingUntracked = await RefreshTemplateAsync(existing, cancellationToken);

        for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                return await operation(existingUntracked, cancellationToken);
            }
            catch (Exception ex) when (ShouldRetry(ex) && attempt < MaxRetryAttempts)
            {
                await DelayWithBackoffAsync(attempt, cancellationToken);
                existingUntracked = await RefreshTemplateAsync(existing, cancellationToken);
            }
        }

        // Final attempt without retry logic
        return await operation(existingUntracked, cancellationToken);
    }

    private async Task<DomainQuestionnaireTemplate> RefreshTemplateAsync(
        DomainQuestionnaireTemplate existing,
        CancellationToken cancellationToken)
    {
        var refreshed = await _templateRepository.FirstOrDefaultAsync(
            new QuestionnaireTemplateByNameAndVersionNoTrackingSpec(existing.Name, existing.Version),
            cancellationToken);

        return refreshed ?? throw new InvalidOperationException(
            $"Template '{existing.Name}' v{existing.Version} not found.");
    }

    private static bool ShouldRetry(Exception ex)
    {
        return IsUniqueConstraintViolation(ex) || IsConcurrencyException(ex);
    }

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        var message = ex.Message + (ex.InnerException?.Message ?? string.Empty);
        return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsConcurrencyException(Exception ex)
    {
        var type = ex.GetType();
        var typeName = type.Name;
        return typeName.Contains("Concurrency", StringComparison.OrdinalIgnoreCase) ||
               (ex.InnerException != null && 
                ex.InnerException.GetType().Name.Contains("Concurrency", StringComparison.OrdinalIgnoreCase));
    }

    private static Task DelayWithBackoffAsync(int attempt, CancellationToken cancellationToken)
    {
        var delay = BaseRetryDelayMs * (int)Math.Pow(2, attempt - 1) + Random.Shared.Next(0, 25);
        return Task.Delay(delay, cancellationToken);
    }
}
