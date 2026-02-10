using AutoMapper;
using Microsoft.Extensions.Logging;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Handlers;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Domain.Models;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.QuestionnaireTemplate;

public sealed class QuestionnaireTemplateOrchestrator : IQuestionnaireTemplateOrchestrator
{
    private readonly IRepository<DomainQuestionnaireTemplate> _templateRepository;
    private readonly IRepository<QuestionnaireQuestion> _questionRepository;
    private readonly IRepository<QuestionnaireQuestionOption> _optionRepository;
    private readonly IQuestionnaireVersioningService _versioningService;
    private readonly IQuestionnaireTemplateRetryHandler _retryHandler;
    private readonly IQuestionnaireEntityFactory _entityFactory;
    private readonly IOptionNameNormalizer _nameNormalizer;
    private readonly SectionSyncHandler _sectionSyncHandler;
    private readonly IMapper _mapper;
    private readonly ILogger<QuestionnaireTemplateOrchestrator> _logger;

    public QuestionnaireTemplateOrchestrator(
        IRepository<DomainQuestionnaireTemplate> templateRepository,
        IRepository<QuestionnaireQuestion> questionRepository,
        IRepository<QuestionnaireQuestionOption> optionRepository,
        IQuestionnaireVersioningService versioningService,
        IQuestionnaireTemplateRetryHandler retryHandler,
        IQuestionnaireEntityFactory entityFactory,
        IOptionNameNormalizer nameNormalizer,
        SectionSyncHandler sectionSyncHandler,
        IMapper mapper,
        ILogger<QuestionnaireTemplateOrchestrator> logger)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        _optionRepository = optionRepository ?? throw new ArgumentNullException(nameof(optionRepository));
        _versioningService = versioningService ?? throw new ArgumentNullException(nameof(versioningService));
        _retryHandler = retryHandler ?? throw new ArgumentNullException(nameof(retryHandler));
        _entityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
        _nameNormalizer = nameNormalizer ?? throw new ArgumentNullException(nameof(nameNormalizer));
        _sectionSyncHandler = sectionSyncHandler ?? throw new ArgumentNullException(nameof(sectionSyncHandler));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QuestionnaireTemplateDto> VersionTemplateAsync(
        DomainQuestionnaireTemplate existing,
        QuestionnaireTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryHandler.ExecuteWithRetryAsync(
                existing,
                async (existingUntracked, ct) => await VersionTemplateInternalAsync(existingUntracked, dto, ct),
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"VersionTemplateAsync: Error versioning template: Name={existing.Name}, Version={existing.Version}");
            throw new InvalidOperationException($"Failed to version template '{existing.Name}' v{existing.Version}.", ex);
        }
    }

    private async Task<QuestionnaireTemplateDto> VersionTemplateInternalAsync(
        DomainQuestionnaireTemplate existing,
        QuestionnaireTemplateDto dto,
        CancellationToken cancellationToken)
    {
        _templateRepository.ClearChangeTracker();

        var nextVersion = await CalculateNextTemplateVersionAsync(dto.Name, dto.Version, cancellationToken);
        
        var existingVersion = await _templateRepository.FirstOrDefaultAsync(
            new QuestionnaireTemplateByNameAndVersionNoTrackingSpec(dto.Name, nextVersion), cancellationToken);
        
        if (existingVersion != null)
        {
            return _mapper.Map<QuestionnaireTemplateDto>(existingVersion);
        }

        var newTemplate = _entityFactory.CreateTemplate(dto, nextVersion);

        foreach (var sectionDto in dto.Sections ?? new List<QuestionnaireSectionDto>())
        {
            var oldSection = existing.Sections.FirstOrDefault(s => s.Order == sectionDto.Order);
            var newSection = await VersionSectionAsync(
                oldSection, sectionDto, newTemplate.Name, newTemplate.Version, cancellationToken);
            newTemplate.Sections.Add(newSection);
        }

        await _templateRepository.AddAsync(newTemplate, cancellationToken);
        await _templateRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QuestionnaireTemplateDto>(newTemplate);
    }


    public async Task<QuestionnaireTemplateDto?> SyncSectionsAsync(
        DomainQuestionnaireTemplate existing,
        List<QuestionnaireSectionDto> incoming,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _sectionSyncHandler.SyncSectionsAsync(existing, incoming, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"SyncSectionsAsync: Error syncing sections: TemplateName={existing.Name}, TemplateVersion={existing.Version}");
            throw new InvalidOperationException($"Failed to sync sections for template '{existing.Name}' v{existing.Version}.", ex);
        }
    }

    public async Task<QuestionnaireTemplateDto> VersionTemplateForQuestionAsync(
        DomainQuestionnaireTemplate template,
        QuestionnaireQuestion editedQuestion,
        QuestionnaireQuestionDto questionDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryHandler.ExecuteWithRetryAsync(
                template,
                async (templateUntracked, ct) => await VersionTemplateForQuestionInternalAsync(templateUntracked, editedQuestion, questionDto, ct),
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"VersionTemplateForQuestionAsync: Error versioning template for question: TemplateName={template.Name}, TemplateVersion={template.Version}, QuestionName={editedQuestion.Name}");
            throw new InvalidOperationException($"Failed to version template '{template.Name}' v{template.Version} for question '{editedQuestion.Name}'.", ex);
        }
    }

    private async Task<QuestionnaireTemplateDto> VersionTemplateForQuestionInternalAsync(
        DomainQuestionnaireTemplate template,
        QuestionnaireQuestion editedQuestion,
        QuestionnaireQuestionDto questionDto,
        CancellationToken cancellationToken)
    {
        _templateRepository.ClearChangeTracker();

        var nextVersion = await CalculateNextTemplateVersionAsync(template.Name, template.Version, cancellationToken);
        
        var existingVersion = await _templateRepository.FirstOrDefaultAsync(
            new QuestionnaireTemplateByNameAndVersionNoTrackingSpec(template.Name, nextVersion), cancellationToken);
        
        if (existingVersion != null)
        {
            return _mapper.Map<QuestionnaireTemplateDto>(existingVersion);
        }

        var newTemplate = _entityFactory.CreateTemplateFromExisting(template, nextVersion);

        foreach (var section in template.Sections)
        {
            var newSection = await VersionSectionForQuestionEditAsync(
                section, editedQuestion, questionDto, newTemplate.Name, newTemplate.Version, cancellationToken);
            newTemplate.Sections.Add(newSection);
        }

        await _templateRepository.AddAsync(newTemplate, cancellationToken);
        await _templateRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QuestionnaireTemplateDto>(newTemplate);
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

    /// <summary>
    /// Calculates the next version number for a template.
    /// This method is extracted for reusability and to ensure consistent version calculation logic.
    /// </summary>
    private async Task<int> CalculateNextTemplateVersionAsync(string templateName, int currentVersion, CancellationToken cancellationToken)
    {
        var latest = await _templateRepository.FirstOrDefaultAsync(
            new QuestionnaireTemplateLatestByNameNoTrackingSpec(templateName), cancellationToken);
        return (latest?.Version ?? currentVersion) + 1;
    }


    /// <summary>
    /// Versions a section with all its questions and options.
    /// </summary>
    private async Task<QuestionnaireSection> VersionSectionAsync(
        QuestionnaireSection? oldSection,
        QuestionnaireSectionDto sectionDto,
        string templateName,
        int templateVersion,
        CancellationToken cancellationToken)
    {
        var newSection = new QuestionnaireSection
        {
            Id = Guid.NewGuid(),
            QuestionnaireTemplateName = templateName,
            QuestionnaireTemplateVersion = templateVersion,
            Order = sectionDto.Order,
            Title = sectionDto.Title?.Trim() ?? string.Empty,
            Description = sectionDto.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Questions = new List<QuestionnaireQuestion>()
        };

        foreach (var questionDto in sectionDto.Questions ?? new List<QuestionnaireQuestionDto>())
        {
            var questionName = questionDto.Name;
            var oldQuestion = oldSection?.Questions.FirstOrDefault(q => q.Name == questionName);
            
            // Create question without calling AddAsync - let EF Core handle it through navigation
            var newQuestion = await VersionOrCreateQuestionForSectionAsync(
                oldQuestion, questionDto, newSection.Id, cancellationToken);
            
            newSection.Questions.Add(newQuestion);
        }

        return newSection;
    }

    /// <summary>
    /// Versions a section when editing a specific question.
    /// </summary>
    private async Task<QuestionnaireSection> VersionSectionForQuestionEditAsync(
        QuestionnaireSection section,
        QuestionnaireQuestion editedQuestion,
        QuestionnaireQuestionDto questionDto,
        string templateName,
        int templateVersion,
        CancellationToken cancellationToken)
    {
        var newSection = new QuestionnaireSection
        {
            Id = Guid.NewGuid(),
            QuestionnaireTemplateName = templateName,
            QuestionnaireTemplateVersion = templateVersion,
            Order = section.Order,
            Title = section.Title,
            Description = section.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Questions = new List<QuestionnaireQuestion>()
        };

        // Initialize questions collection
        newSection.Questions = new List<QuestionnaireQuestion>();

        foreach (var question in section.Questions)
        {
            QuestionnaireQuestion newQuestion;
            
            if (question.Name == editedQuestion.Name)
            {
                // This is the edited question - version it with changes
                newQuestion = await _versioningService.VersionQuestionAsync(question, newSection.Id, cancellationToken);
                ApplyQuestionDtoChanges(newQuestion, questionDto);
            }
            else
            {
                // Other questions - version them without changes
                newQuestion = await _versioningService.VersionQuestionAsync(question, newSection.Id, cancellationToken);
            }

            // Initialize options collection
            newQuestion.Options = new List<QuestionnaireQuestionOption>();

            // Version all options for this question (don't call AddAsync - use navigation properties)
            foreach (var option in question.Options)
            {
                var newOption = await _versioningService.VersionOptionAsync(
                    option, newQuestion.Name, newQuestion.Version, cancellationToken);
                
                // Apply changes if this is the edited question
                if (question.Name == editedQuestion.Name)
                {
                    var optionDto = (questionDto.Options ?? new List<QuestionnaireOptionDto>())
                        .FirstOrDefault(o => o.Name == option.Name);
                    if (optionDto != null)
                    {
                        ApplyOptionDtoChanges(newOption, optionDto);
                    }
                }
                
                // Add to collection - EF Core will track it when section is added to template
                newQuestion.Options.Add(newOption);
            }

            // If this is the edited question, also include any NEW options coming from the DTO (e.g. newly added in UI).
            // These options do not exist in the source question.Options collection, so we must create them explicitly.
            if (question.Name == editedQuestion.Name)
            {
                var existingNames = new HashSet<string>(
                    question.Options.Select(o => o.Name),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var incomingOpt in questionDto.Options ?? new List<QuestionnaireOptionDto>())
                {
                    if (!string.IsNullOrWhiteSpace(incomingOpt.Name) && existingNames.Contains(incomingOpt.Name))
                        continue;

                    var normalizedName = _nameNormalizer.NormalizeOptionName(incomingOpt, newQuestion);
                    var uniqueName = await _nameNormalizer.EnsureUniqueOptionNameV1Async(normalizedName, cancellationToken);
                    var dtoWithName = new QuestionnaireOptionDto
                    {
                        Name = uniqueName,
                        Version = incomingOpt.Version,
                        Order = incomingOpt.Order,
                        Label = incomingOpt.Label,
                        MediaUrl = incomingOpt.MediaUrl,
                        MediaFileId = incomingOpt.MediaFileId,
                        IsCorrect = incomingOpt.IsCorrect,
                        Score = incomingOpt.Score,
                        Weight = incomingOpt.Weight,
                        Wa = incomingOpt.Wa
                    };

                    var created = _entityFactory.CreateOption(dtoWithName, newQuestion);
                    newQuestion.Options.Add(created);
                }
            }

            newSection.Questions.Add(newQuestion);
        }

        return newSection;
    }

    /// <summary>
    /// Versions an existing question or creates a new one for versioning scenarios.
    /// Does NOT call AddAsync - relies on EF Core navigation properties for tracking.
    /// </summary>
    private async Task<QuestionnaireQuestion> VersionOrCreateQuestionForSectionAsync(
        QuestionnaireQuestion? oldQuestion,
        QuestionnaireQuestionDto questionDto,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        QuestionnaireQuestion newQuestion;
        
        if (oldQuestion != null)
        {
            newQuestion = await _versioningService.VersionQuestionAsync(oldQuestion, sectionId, cancellationToken);
            ApplyQuestionDtoChanges(newQuestion, questionDto);
        }
        else
        {
            newQuestion = _entityFactory.CreateQuestion(questionDto, sectionId);
        }

        // Initialize options collection
        newQuestion.Options = new List<QuestionnaireQuestionOption>();

        // Version or create options (don't call AddAsync - use navigation properties)
        foreach (var optionDto in questionDto.Options ?? new List<QuestionnaireOptionDto>())
        {
            var optionName = optionDto.Name;
            var oldOption = oldQuestion?.Options.FirstOrDefault(o => o.Name == optionName);
            
            QuestionnaireQuestionOption newOption;
            if (oldOption != null)
            {
                newOption = await _versioningService.VersionOptionAsync(
                    oldOption, newQuestion.Name, newQuestion.Version, cancellationToken);
                ApplyOptionDtoChanges(newOption, optionDto);
            }
            else
            {
                // New option: ensure (Name, Version=1) uniqueness before creating the entity.
                var normalizedName = _nameNormalizer.NormalizeOptionName(optionDto, newQuestion);
                var uniqueName = await _nameNormalizer.EnsureUniqueOptionNameV1Async(normalizedName, cancellationToken);
                var dtoWithName = new QuestionnaireOptionDto
                {
                    Name = uniqueName,
                    Version = optionDto.Version,
                    Order = optionDto.Order,
                    Label = optionDto.Label,
                    MediaUrl = optionDto.MediaUrl,
                    MediaFileId = optionDto.MediaFileId,
                    IsCorrect = optionDto.IsCorrect,
                    Score = optionDto.Score,
                    Weight = optionDto.Weight,
                    Wa = optionDto.Wa
                };
                newOption = _entityFactory.CreateOption(dtoWithName, newQuestion);
            }

            // Add to collection - EF Core will track it when section is added to template
            newQuestion.Options.Add(newOption);
        }

        return newQuestion;
    }

    private void LogError(Exception ex, string message)
    {
        _logger.LogError(ex, message);
    }
}
