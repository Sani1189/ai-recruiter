using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Domain.Models;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.QuestionnaireTemplate.Handlers;

/// <summary>
/// Handles synchronization of questionnaire questions.
/// </summary>
public sealed class QuestionSyncHandler
{
    private readonly IRepository<QuestionnaireQuestion> _questionRepository;
    private readonly IRepository<QuestionnaireQuestionOption> _optionRepository;
    private readonly IRepository<QuestionnaireCandidateSubmission> _submissionRepository;
    private readonly IQuestionnaireVersioningService _versioningService;
    private readonly IQuestionnaireEntityFactory _entityFactory;
    private readonly IOptionNameNormalizer _nameNormalizer;
    private readonly OptionSyncHandler _optionSyncHandler;

    public QuestionSyncHandler(
        IRepository<QuestionnaireQuestion> questionRepository,
        IRepository<QuestionnaireQuestionOption> optionRepository,
        IRepository<QuestionnaireCandidateSubmission> submissionRepository,
        IQuestionnaireVersioningService versioningService,
        IQuestionnaireEntityFactory entityFactory,
        IOptionNameNormalizer nameNormalizer,
        OptionSyncHandler optionSyncHandler)
    {
        _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        _optionRepository = optionRepository ?? throw new ArgumentNullException(nameof(optionRepository));
        _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        _versioningService = versioningService ?? throw new ArgumentNullException(nameof(versioningService));
        _entityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
        _nameNormalizer = nameNormalizer ?? throw new ArgumentNullException(nameof(nameNormalizer));
        _optionSyncHandler = optionSyncHandler ?? throw new ArgumentNullException(nameof(optionSyncHandler));
    }

    public async Task<QuestionSyncResult> SyncQuestionsAsync(
        QuestionnaireSection section,
        List<QuestionnaireQuestionDto> incoming,
        DomainQuestionnaireTemplate template,
        CancellationToken cancellationToken)
    {
        var isTemplateInUse = await IsTemplateInUseAsync(template.Name, template.Version, cancellationToken);

        var incomingByName = incoming.ToDictionary(x => x.Name, x => x);
        var existingByName = section.Questions.ToDictionary(x => x.Name, x => x);

        await RemoveQuestionsNotInIncomingAsync(section, incomingByName, isTemplateInUse, cancellationToken);

        foreach (var qDto in incoming)
        {
            var result = await ProcessQuestionAsync(
                qDto, section, template, existingByName, isTemplateInUse, cancellationToken);

            if (result.VersionedTemplate != null)
                return result;
        }

        return new QuestionSyncResult(VersionedTemplate: null);
    }

    private async Task RemoveQuestionsNotInIncomingAsync(
        QuestionnaireSection section,
        Dictionary<string, QuestionnaireQuestionDto> incomingByName,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        var toRemove = section.Questions.Where(q => !incomingByName.ContainsKey(q.Name)).ToList();
        foreach (var question in toRemove)
        {
            if (isTemplateInUse)
            {
                throw new InvalidOperationException(
                    "Cannot remove question. Template is in use. Please version the template first.");
            }
            section.Questions.Remove(question);
        }
    }

    private async Task<QuestionSyncResult> ProcessQuestionAsync(
        QuestionnaireQuestionDto qDto,
        QuestionnaireSection section,
        DomainQuestionnaireTemplate template,
        Dictionary<string, QuestionnaireQuestion> existingByName,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        var questionEntity = existingByName.TryGetValue(qDto.Name, out var existing)
            ? existing
            : await CreateNewQuestionAsync(qDto, section, isTemplateInUse, cancellationToken);

        if (questionEntity == null)
        {
            // Question was versioned, skip option sync
            return new QuestionSyncResult(VersionedTemplate: null);
        }

        var questionChanged = QuestionnaireTemplateChangeDetector.HasQuestionChanged(questionEntity, qDto);
        if (questionChanged && isTemplateInUse)
        {
            await VersionQuestionWithOptionsAndQuestionChangesAsync(questionEntity, qDto, section, cancellationToken);
            return new QuestionSyncResult(VersionedTemplate: null);
        }

        if (questionChanged)
        {
            ApplyQuestionDtoChanges(questionEntity, qDto);
        }
        else
        {
            ApplyQuestionDtoChanges(questionEntity, qDto);
        }

        // Sync options
        var optionsResult = await _optionSyncHandler.SyncOptionsAsync(
            questionEntity,
            qDto.Options ?? new List<QuestionnaireOptionDto>(),
            section,
            template,
            cancellationToken);

        if (optionsResult.VersionedTemplate != null)
            return new QuestionSyncResult(VersionedTemplate: optionsResult.VersionedTemplate);

        if (optionsResult.QuestionVersioned)
            return new QuestionSyncResult(VersionedTemplate: null);

        return new QuestionSyncResult(VersionedTemplate: null);
    }

    private async Task<QuestionnaireQuestion?> CreateNewQuestionAsync(
        QuestionnaireQuestionDto qDto,
        QuestionnaireSection section,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        if (isTemplateInUse)
        {
            throw new InvalidOperationException(
                "Cannot add new question. Template is in use. Please version the template first.");
        }

        var newQuestion = _entityFactory.CreateQuestion(qDto, section.Id);
        await _questionRepository.AddAsync(newQuestion, cancellationToken);
        section.Questions.Add(newQuestion);

        return newQuestion;
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
            var normalizedName = NormalizeOptionName(optDto, newQuestion);
            QuestionnaireQuestionOption newOption;

            if (existingOptionsByKey.TryGetValue(optDto.Name, out var exact) ||
                existingOptionsByKey.TryGetValue(normalizedName, out exact))
            {
                newOption = await _versioningService.VersionOptionAsync(exact, newQuestion.Name, newQuestion.Version, cancellationToken);
                ApplyOptionDtoChanges(newOption, optDto);
            }
            else
            {
                // This should not happen in this context, but handle it
                var uniqueName = await EnsureUniqueOptionNameV1Async(normalizedName, cancellationToken);
                var dtoWithName = CreateOptionDtoWithName(optDto, uniqueName);
                newOption = _entityFactory.CreateOption(dtoWithName, newQuestion);
            }

            await _optionRepository.AddAsync(newOption, cancellationToken);
            newQuestion.Options.Add(newOption);
        }

        section.Questions.Add(newQuestion);
    }

    private static Dictionary<string, QuestionnaireQuestionOption> BuildExistingOptionsMap(QuestionnaireQuestion question)
    {
        var existingOptionsByKey = new Dictionary<string, QuestionnaireQuestionOption>(StringComparer.OrdinalIgnoreCase);
        foreach (var opt in question.Options)
        {
            existingOptionsByKey[opt.Name] = opt;
            if (opt.Name.StartsWith(question.Name + "_", StringComparison.OrdinalIgnoreCase))
            {
                var baseName = opt.Name.Substring(question.Name.Length + 1);
                if (!existingOptionsByKey.ContainsKey(baseName))
                    existingOptionsByKey[baseName] = opt;
            }
        }
        return existingOptionsByKey;
    }

    private string NormalizeOptionName(QuestionnaireOptionDto optDto, QuestionnaireQuestion question)
    {
        return _nameNormalizer.NormalizeOptionName(optDto, question);
    }

    private async Task<string> EnsureUniqueOptionNameV1Async(string normalizedName, CancellationToken cancellationToken)
    {
        return await _nameNormalizer.EnsureUniqueOptionNameV1Async(normalizedName, cancellationToken);
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

    public record QuestionSyncResult(QuestionnaireTemplateDto? VersionedTemplate);
}
