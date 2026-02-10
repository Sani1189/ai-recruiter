using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Interfaces;

/// <summary>
/// Normalizes option names to ensure consistency and uniqueness.
/// </summary>
public interface IOptionNameNormalizer
{
    /// <summary>
    /// Normalizes an option name using the same logic as entity creation.
    /// </summary>
    string NormalizeOptionName(QuestionnaireOptionDto optionDto, QuestionnaireQuestion question);

    /// <summary>
    /// Ensures the option name is globally unique for Version=1.
    /// </summary>
    Task<string> EnsureUniqueOptionNameV1Async(string desiredName, CancellationToken cancellationToken = default);
}
