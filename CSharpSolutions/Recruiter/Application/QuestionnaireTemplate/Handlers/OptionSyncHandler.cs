using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Domain.Models;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.QuestionnaireTemplate.Handlers;

/// <summary>
/// Handles synchronization of questionnaire question options.
/// </summary>
public sealed class OptionSyncHandler
{
    private readonly IRepository<QuestionnaireQuestionOption> _optionRepository;
    private readonly IRepository<QuestionnaireQuestion> _questionRepository;
    private readonly IRepository<QuestionnaireCandidateSubmission> _submissionRepository;
    private readonly IQuestionnaireVersioningService _versioningService;
    private readonly IOptionNameNormalizer _nameNormalizer;
    private readonly IQuestionnaireEntityFactory _entityFactory;

    public OptionSyncHandler(
        IRepository<QuestionnaireQuestionOption> optionRepository,
        IRepository<QuestionnaireQuestion> questionRepository,
        IRepository<QuestionnaireCandidateSubmission> submissionRepository,
        IQuestionnaireVersioningService versioningService,
        IOptionNameNormalizer nameNormalizer,
        IQuestionnaireEntityFactory entityFactory)
    {
        _optionRepository = optionRepository ?? throw new ArgumentNullException(nameof(optionRepository));
        _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        _versioningService = versioningService ?? throw new ArgumentNullException(nameof(versioningService));
        _nameNormalizer = nameNormalizer ?? throw new ArgumentNullException(nameof(nameNormalizer));
        _entityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
    }

    public async Task<OptionSyncResult> SyncOptionsAsync(
        QuestionnaireQuestion question,
        List<QuestionnaireOptionDto> incoming,
        QuestionnaireSection section,
        DomainQuestionnaireTemplate template,
        CancellationToken cancellationToken)
    {
        var isTemplateInUse = await IsTemplateInUseAsync(template.Name, template.Version, cancellationToken);

        var incomingNormalized = NormalizeIncomingOptions(incoming, question);
        var incomingByNormalizedName = incomingNormalized.ToDictionary(x => x.NormalizedName, x => x.Dto);
        var existingByName = BuildExistingOptionsMap(question);

        await RemoveOptionsNotInIncomingAsync(question, incomingByNormalizedName, isTemplateInUse, cancellationToken);

        foreach (var item in incomingNormalized)
        {
            var result = await ProcessOptionAsync(
                item.Dto, item.NormalizedName, question, section, template, existingByName, isTemplateInUse, cancellationToken);

            if (result.VersionedTemplate != null)
                return new OptionSyncResult(VersionedTemplate: result.VersionedTemplate, QuestionVersioned: false);

            if (result.QuestionVersioned)
                return new OptionSyncResult(VersionedTemplate: null, QuestionVersioned: true);
        }

        return new OptionSyncResult(VersionedTemplate: null, QuestionVersioned: false);
    }

    private List<(QuestionnaireOptionDto Dto, string NormalizedName)> NormalizeIncomingOptions(
        List<QuestionnaireOptionDto> incoming,
        QuestionnaireQuestion question)
    {
        return incoming
            .Select(dto => (Dto: dto, NormalizedName: _nameNormalizer.NormalizeOptionName(dto, question)))
            .ToList();
    }

    private static Dictionary<string, QuestionnaireQuestionOption> BuildExistingOptionsMap(QuestionnaireQuestion question)
    {
        var existingByName = new Dictionary<string, QuestionnaireQuestionOption>(StringComparer.OrdinalIgnoreCase);
        foreach (var opt in question.Options)
        {
            existingByName[opt.Name] = opt;

            if (opt.Name.StartsWith(question.Name + "_", StringComparison.OrdinalIgnoreCase))
            {
                var baseName = opt.Name.Substring(question.Name.Length + 1);
                if (!existingByName.ContainsKey(baseName))
                {
                    existingByName[baseName] = opt;
                }
            }
        }
        return existingByName;
    }

    private async Task RemoveOptionsNotInIncomingAsync(
        QuestionnaireQuestion question,
        Dictionary<string, QuestionnaireOptionDto> incomingByNormalizedName,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        var toRemove = question.Options
            .Where(o => !incomingByNormalizedName.ContainsKey(o.Name))
            .ToList();

        foreach (var option in toRemove)
        {
            if (isTemplateInUse)
            {
                throw new InvalidOperationException(
                    "Cannot remove option. Template is in use. Please version the template first.");
            }

            await _optionRepository.DeleteAsync(option, cancellationToken);
            question.Options.Remove(option);
        }
    }

    private async Task<OptionSyncResult> ProcessOptionAsync(
        QuestionnaireOptionDto optionDto,
        string normalizedName,
        QuestionnaireQuestion question,
        QuestionnaireSection section,
        DomainQuestionnaireTemplate template,
        Dictionary<string, QuestionnaireQuestionOption> existingByName,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        var optionEntity = existingByName.TryGetValue(normalizedName, out var existing)
            ? existing
            : await CreateNewOptionAsync(optionDto, normalizedName, question, section, template, isTemplateInUse, cancellationToken);

        if (optionEntity == null)
        {
            // Question was versioned, return early
            return new OptionSyncResult(VersionedTemplate: null, QuestionVersioned: true);
        }

        var optionChanged = QuestionnaireTemplateChangeDetector.HasOptionChanged(optionEntity, optionDto);
        if (optionChanged && isTemplateInUse)
        {
            await VersionQuestionForOptionChangeAsync(question, optionDto, section, cancellationToken);
            return new OptionSyncResult(VersionedTemplate: null, QuestionVersioned: true);
        }

        ApplyOptionDtoChanges(optionEntity, optionDto);
        return new OptionSyncResult(VersionedTemplate: null, QuestionVersioned: false);
    }

    private async Task<QuestionnaireQuestionOption?> CreateNewOptionAsync(
        QuestionnaireOptionDto optionDto,
        string normalizedName,
        QuestionnaireQuestion question,
        QuestionnaireSection section,
        DomainQuestionnaireTemplate template,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        if (isTemplateInUse)
        {
            await VersionQuestionForNewOptionAsync(question, optionDto, section, cancellationToken);
            return null; // Question was versioned
        }

        var uniqueOptionName = await _nameNormalizer.EnsureUniqueOptionNameV1Async(normalizedName, cancellationToken);
        var dtoWithNormalizedName = CreateOptionDtoWithName(optionDto, uniqueOptionName);
        var newOption = _entityFactory.CreateOption(dtoWithNormalizedName, question);

        await _optionRepository.AddAsync(newOption, cancellationToken);
        question.Options.Add(newOption);

        return newOption;
    }

    private async Task VersionQuestionForNewOptionAsync(
        QuestionnaireQuestion question,
        QuestionnaireOptionDto newOptionDto,
        QuestionnaireSection section,
        CancellationToken cancellationToken)
    {
        var allOptions = question.Options.Select(o => MapToDto(o)).ToList();
        allOptions.Add(newOptionDto);

        var questionDto = CreateQuestionDtoFromEntity(question, allOptions);
        await VersionQuestionWithOptionsAndQuestionChangesAsync(question, questionDto, section, cancellationToken);
    }

    private async Task VersionQuestionForOptionChangeAsync(
        QuestionnaireQuestion question,
        QuestionnaireOptionDto changedOptionDto,
        QuestionnaireSection section,
        CancellationToken cancellationToken)
    {
        var allOptions = question.Options.Select(o => MapToDto(o)).ToList();
        var existingOptionIndex = allOptions.FindIndex(o => o.Name == changedOptionDto.Name);
        if (existingOptionIndex >= 0)
        {
            allOptions[existingOptionIndex] = changedOptionDto;
        }

        var questionDto = CreateQuestionDtoFromEntity(question, allOptions);
        await VersionQuestionWithOptionsAndQuestionChangesAsync(question, questionDto, section, cancellationToken);
    }

    private async Task VersionQuestionWithOptionsAndQuestionChangesAsync(
        QuestionnaireQuestion current,
        QuestionnaireQuestionDto incoming,
        QuestionnaireSection section,
        CancellationToken cancellationToken)
    {
        current.IsActive = false;
        current.UpdatedAt = DateTimeOffset.UtcNow;

        var newQuestion = await _versioningService.VersionQuestionAsync(current, section.Id, cancellationToken);
        ApplyQuestionDtoChanges(newQuestion, incoming);
        await _questionRepository.AddAsync(newQuestion, cancellationToken);

        newQuestion.Options = new List<QuestionnaireQuestionOption>();

        var existingOptionsByKey = BuildExistingOptionsMap(current);

        foreach (var optDto in incoming.Options ?? new List<QuestionnaireOptionDto>())
        {
            var normalizedName = _nameNormalizer.NormalizeOptionName(optDto, newQuestion);
            QuestionnaireQuestionOption newOption;

            if (existingOptionsByKey.TryGetValue(optDto.Name, out var exact) ||
                existingOptionsByKey.TryGetValue(normalizedName, out exact))
            {
                newOption = await _versioningService.VersionOptionAsync(exact, newQuestion.Name, newQuestion.Version, cancellationToken);
                ApplyOptionDtoChanges(newOption, optDto);
            }
            else
            {
                var uniqueName = await _nameNormalizer.EnsureUniqueOptionNameV1Async(normalizedName, cancellationToken);
                var dtoWithName = CreateOptionDtoWithName(optDto, uniqueName);
                newOption = _entityFactory.CreateOption(dtoWithName, newQuestion);
            }

            await _optionRepository.AddAsync(newOption, cancellationToken);
            newQuestion.Options.Add(newOption);
        }

        section.Questions.Add(newQuestion);
    }

    private static QuestionnaireOptionDto MapToDto(QuestionnaireQuestionOption option)
    {
        return new QuestionnaireOptionDto
        {
            Name = option.Name,
            Version = option.Version,
            Order = option.Order,
            Label = option.Label,
            MediaUrl = option.MediaUrl,
            MediaFileId = option.MediaFileId,
            IsCorrect = option.IsCorrect,
            Score = option.Score,
            Weight = option.Weight,
            Wa = option.Wa
        };
    }

    private static QuestionnaireQuestionDto CreateQuestionDtoFromEntity(
        QuestionnaireQuestion question,
        List<QuestionnaireOptionDto> options)
    {
        return new QuestionnaireQuestionDto
        {
            Name = question.Name,
            Version = question.Version,
            Order = question.Order,
            QuestionType = question.QuestionType.ToString(),
            IsRequired = question.IsRequired,
            PromptText = question.QuestionText,
            MediaUrl = question.MediaUrl,
            MediaFileId = question.MediaFileId,
            Ws = question.Ws,
            TraitKey = question.TraitKey,
            Options = options
        };
    }

    private static QuestionnaireOptionDto CreateOptionDtoWithName(QuestionnaireOptionDto source, string name)
    {
        return new QuestionnaireOptionDto
        {
            Name = name,
            Version = source.Version,
            Order = source.Order,
            Label = source.Label,
            MediaUrl = source.MediaUrl,
            MediaFileId = source.MediaFileId,
            IsCorrect = source.IsCorrect,
            Score = source.Score,
            Weight = source.Weight,
            Wa = source.Wa
        };
    }

    private static void ApplyQuestionDtoChanges(QuestionnaireQuestion question, QuestionnaireQuestionDto dto)
    {
        question.Order = dto.Order;
        if (Enum.TryParse<Domain.Enums.QuestionnaireQuestionTypeEnum>(dto.QuestionType, true, out var questionType))
            question.QuestionType = questionType;
        question.QuestionText = dto.PromptText?.Trim() ?? string.Empty;
        question.IsRequired = dto.IsRequired;
        question.TraitKey = dto.TraitKey;
        question.Ws = dto.Ws;
        question.MediaUrl = dto.MediaUrl;
        question.MediaFileId = dto.MediaFileId;
        question.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ApplyOptionDtoChanges(QuestionnaireQuestionOption option, QuestionnaireOptionDto dto)
    {
        option.Order = dto.Order;
        option.Label = dto.Label?.Trim() ?? string.Empty;
        option.MediaUrl = dto.MediaUrl;
        option.MediaFileId = dto.MediaFileId;
        option.IsCorrect = dto.IsCorrect;
        option.Score = dto.Score;
        option.Weight = dto.Weight;
        option.Wa = dto.Wa;
        option.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private async Task<bool> IsTemplateInUseAsync(string name, int version, CancellationToken cancellationToken)
    {
        var submissionCount = await _submissionRepository.CountAsync(
            new TemplateInUseBySubmissionsSpec(name, version), cancellationToken);
        return submissionCount > 0;
    }

    public record OptionSyncResult(QuestionnaireTemplateDto? VersionedTemplate, bool QuestionVersioned);
}
