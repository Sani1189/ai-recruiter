using Recruiter.Application.Common.Helpers;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate;

/// <summary>
/// Normalizes option names to ensure consistency and uniqueness.
/// </summary>
public sealed class OptionNameNormalizer : IOptionNameNormalizer
{
    private readonly IRepository<QuestionnaireQuestionOption> _optionRepository;

    public OptionNameNormalizer(IRepository<QuestionnaireQuestionOption> optionRepository)
    {
        _optionRepository = optionRepository ?? throw new ArgumentNullException(nameof(optionRepository));
    }

    public string NormalizeOptionName(QuestionnaireOptionDto optionDto, QuestionnaireQuestion question)
    {
        if (string.IsNullOrWhiteSpace(optionDto.Name))
        {
            var label = optionDto.Label?.Trim() ?? string.Empty;
            return $"{question.Name}_{StringHelper.Slugify(label)}";
        }

        var baseName = optionDto.Name.Trim();
        if (baseName.StartsWith("option_", StringComparison.OrdinalIgnoreCase) ||
            baseName.StartsWith("opt_", StringComparison.OrdinalIgnoreCase))
        {
            return $"{question.Name}_{baseName}";
        }

        return baseName;
    }

    public async Task<string> EnsureUniqueOptionNameV1Async(string desiredName, CancellationToken cancellationToken = default)
    {
        var candidate = desiredName.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            throw new InvalidOperationException("Option name cannot be empty.");

        // Fast path: check if name is available
        var existing = await _optionRepository.FirstOrDefaultAsync(
            new OptionByNameAndVersionSpec(candidate, 1), cancellationToken);
        if (existing == null)
            return candidate;

        // Add deterministic suffix until unique
        for (var i = 2; i <= 50; i++)
        {
            var withSuffix = $"{candidate}_{i}";
            existing = await _optionRepository.FirstOrDefaultAsync(
                new OptionByNameAndVersionSpec(withSuffix, 1), cancellationToken);
            if (existing == null)
                return withSuffix;
        }

        // Extremely unlikely fallback
        var random = Guid.NewGuid().ToString("N")[..8];
        return $"{candidate}_{random}";
    }
}
