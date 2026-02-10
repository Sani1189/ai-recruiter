using Microsoft.Extensions.Caching.Memory;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate;

public class QuestionnaireVersioningService : IQuestionnaireVersioningService
{
    private readonly IRepository<QuestionnaireQuestion> _questionRepository;
    private readonly IRepository<QuestionnaireQuestionOption> _optionRepository;
    private readonly IMemoryCache _cache;
    private const int CacheExpirationMinutes = 5;

    public QuestionnaireVersioningService(
        IRepository<QuestionnaireQuestion> questionRepository,
        IRepository<QuestionnaireQuestionOption> optionRepository,
        IMemoryCache cache)
    {
        _questionRepository = questionRepository;
        _optionRepository = optionRepository;
        _cache = cache;
    }

    public async Task<QuestionnaireQuestion> VersionQuestionAsync(
        QuestionnaireQuestion source, 
        Guid newSectionId, 
        CancellationToken cancellationToken = default)
    {
        var latest = await _questionRepository.FirstOrDefaultAsync(
            new QuestionLatestByNameSpec(source.Name), cancellationToken);
        
        var nextVersion = (latest?.Version ?? source.Version) + 1;

        var newQuestion = new QuestionnaireQuestion
        {
            Name = source.Name,
            Version = nextVersion,
            QuestionnaireSectionId = newSectionId,
            Order = source.Order,
            QuestionType = source.QuestionType,
            QuestionText = source.QuestionText,
            IsRequired = source.IsRequired,
            TraitKey = source.TraitKey,
            Ws = source.Ws,
            MediaFileId = source.MediaFileId,
            MediaUrl = source.MediaUrl,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Options = new List<QuestionnaireQuestionOption>()
        };

        InvalidateCache(source.Name, "question");
        return newQuestion;
    }

    public async Task<QuestionnaireQuestionOption> VersionOptionAsync(
        QuestionnaireQuestionOption source, 
        string newQuestionName, 
        int newQuestionVersion, 
        CancellationToken cancellationToken = default)
    {
        var latest = await _optionRepository.FirstOrDefaultAsync(
            new OptionLatestByNameSpec(source.Name), cancellationToken);
        
        var nextVersion = (latest?.Version ?? source.Version) + 1;

        var newOption = new QuestionnaireQuestionOption
        {
            Name = source.Name,
            Version = nextVersion,
            QuestionnaireQuestionName = newQuestionName,
            QuestionnaireQuestionVersion = newQuestionVersion,
            Order = source.Order,
            Label = source.Label,
            MediaFileId = source.MediaFileId,
            MediaUrl = source.MediaUrl,
            IsCorrect = source.IsCorrect,
            Score = source.Score,
            Weight = source.Weight,
            Wa = source.Wa,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        InvalidateCache(source.Name, "option");
        return newOption;
    }

    public async Task<Dictionary<string, int>> GetLatestQuestionVersionsAsync(
        IEnumerable<string> questionNames, 
        CancellationToken cancellationToken = default)
    {
        var names = questionNames.Distinct().ToList();
        if (!names.Any())
            return new Dictionary<string, int>();

        var questions = await _questionRepository.ListAsync(
            new QuestionsLatestBatchSpec(names), cancellationToken);

        var versions = questions
            .GroupBy(q => q.Name)
            .ToDictionary(g => g.Key, g => g.First().Version);

        return versions;
    }

    public async Task<Dictionary<string, int>> GetLatestOptionVersionsAsync(
        IEnumerable<string> optionNames, 
        CancellationToken cancellationToken = default)
    {
        var names = optionNames.Distinct().ToList();
        if (!names.Any())
            return new Dictionary<string, int>();

        var options = await _optionRepository.ListAsync(
            new OptionsLatestBatchSpec(names), cancellationToken);

        var versions = options
            .GroupBy(o => o.Name)
            .ToDictionary(g => g.Key, g => g.First().Version);

        return versions;
    }

    public async Task<Domain.Models.QuestionnaireTemplate> ResolveToLatestVersionsAsync(
        Domain.Models.QuestionnaireTemplate template,
        CancellationToken cancellationToken = default)
    {
        var questionNames = template.Sections
            .SelectMany(s => s.Questions)
            .Select(q => q.Name)
            .Distinct()
            .ToList();

        var latestQuestionVersions = await GetLatestQuestionVersionsAsync(questionNames, cancellationToken);

        foreach (var section in template.Sections)
        {
            foreach (var question in section.Questions.ToList())
            {
                if (latestQuestionVersions.TryGetValue(question.Name, out var latestVersion) && latestVersion > question.Version)
                {
                    var latestQuestion = await _questionRepository.FirstOrDefaultAsync(
                        new QuestionLatestByNameWithOptionsSpec(question.Name), cancellationToken);
                    
                    if (latestQuestion != null)
                    {
                        section.Questions.Remove(question);
                        section.Questions.Add(latestQuestion);
                    }
                }
            }

            foreach (var question in section.Questions)
            {
                var optionNames = question.Options.Select(o => o.Name).Distinct().ToList();
                if (optionNames.Any())
                {
                    var latestOptionVersions = await GetLatestOptionVersionsAsync(optionNames, cancellationToken);

                    foreach (var option in question.Options.ToList())
                    {
                        if (latestOptionVersions.TryGetValue(option.Name, out var latestOptVersion) && latestOptVersion > option.Version)
                        {
                            var latestOption = await _optionRepository.FirstOrDefaultAsync(
                                new OptionLatestByNameSpec(option.Name), cancellationToken);
                            
                            if (latestOption != null && latestOption.QuestionnaireQuestionName == question.Name)
                            {
                                question.Options.Remove(option);
                                question.Options.Add(latestOption);
                            }
                        }
                    }
                }
            }
        }

        return template;
    }

    public async Task<List<VersionHistoryItemDto>> GetVersionHistoryAsync(
        string name,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"version_history_{entityType}_{name}";
        if (_cache.TryGetValue(cacheKey, out List<VersionHistoryItemDto>? cachedHistory))
        {
            return cachedHistory ?? new List<VersionHistoryItemDto>();
        }

        List<VersionHistoryItemDto> history = new();

        switch (entityType.ToLowerInvariant())
        {
            case "question":
                var questions = await _questionRepository.ListAsync(
                    new QuestionVersionsByNameSpec(name), cancellationToken);
                history = questions
                    .OrderByDescending(q => q.Version)
                    .Select(q => new VersionHistoryItemDto
                    {
                        Version = q.Version,
                        CreatedAt = q.CreatedAt,
                        UpdatedAt = q.UpdatedAt,
                        CreatedBy = q.CreatedBy,
                        UpdatedBy = q.UpdatedBy,
                        IsDeleted = q.IsDeleted
                    })
                    .ToList();
                break;

            case "option":
                var options = await _optionRepository.ListAsync(
                    new OptionVersionsByNameSpec(name), cancellationToken);
                history = options
                    .OrderByDescending(o => o.Version)
                    .Select(o => new VersionHistoryItemDto
                    {
                        Version = o.Version,
                        CreatedAt = o.CreatedAt,
                        UpdatedAt = o.UpdatedAt,
                        CreatedBy = o.CreatedBy,
                        UpdatedBy = o.UpdatedBy,
                        IsDeleted = o.IsDeleted
                    })
                    .ToList();
                break;
        }

        _cache.Set(cacheKey, history, TimeSpan.FromMinutes(CacheExpirationMinutes));
        return history;
    }

    private void InvalidateCache(string name, string entityType)
    {
        _cache.Remove($"version_history_{entityType}_{name}");
    }
}
