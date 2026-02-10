using Recruiter.Application.QuestionnaireTemplate.Dto;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.QuestionnaireTemplate.Interfaces;

/// <summary>
/// Handles retry logic for concurrent versioning operations with exponential backoff.
/// </summary>
public interface IQuestionnaireTemplateRetryHandler
{
    /// <summary>
    /// Executes a versioning operation with retry logic for concurrent conflicts.
    /// </summary>
    Task<QuestionnaireTemplateDto> ExecuteWithRetryAsync(
        DomainQuestionnaireTemplate existing,
        Func<DomainQuestionnaireTemplate, CancellationToken, Task<QuestionnaireTemplateDto>> operation,
        CancellationToken cancellationToken = default);
}
