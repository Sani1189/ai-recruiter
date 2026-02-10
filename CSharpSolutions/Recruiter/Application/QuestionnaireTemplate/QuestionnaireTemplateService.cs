using Ardalis.Result;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Recruiter.Application.Common.Helpers;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Queries;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate;

public sealed class QuestionnaireTemplateService : IQuestionnaireTemplateService
{
    private const string StatusDraft = "Draft";
    private const string StatusPublished = "Published";
    private const int InitialVersion = 1;
    private const string EntityTypeTemplate = "template";

    private readonly IRepository<Domain.Models.QuestionnaireTemplate> _templateRepository;
    private readonly IQuestionnaireTemplateOrchestrator _orchestrator;
    private readonly IQuestionnaireVersioningService _versioningService;
    private readonly IQuestionnaireEntityFactory _entityFactory;
    private readonly IMapper _mapper;
    private readonly IValidator<QuestionnaireTemplateDto> _validator;
    private readonly QuestionnaireTemplateQueryHandler _queryHandler;
    private readonly ILogger<QuestionnaireTemplateService> _logger;

    public QuestionnaireTemplateService(
        IRepository<Domain.Models.QuestionnaireTemplate> templateRepository,
        IQuestionnaireTemplateOrchestrator orchestrator,
        IQuestionnaireVersioningService versioningService,
        IQuestionnaireEntityFactory entityFactory,
        IMapper mapper,
        IValidator<QuestionnaireTemplateDto> validator,
        QuestionnaireTemplateQueryHandler queryHandler,
        ILogger<QuestionnaireTemplateService> logger)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _versioningService = versioningService ?? throw new ArgumentNullException(nameof(versioningService));
        _entityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<QuestionnaireTemplateDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _queryHandler.GetAllAsync(cancellationToken);

    public async Task<Result<Recruiter.Application.Common.Dto.PagedResult<QuestionnaireTemplateDto>>> GetFilteredAsync(QuestionnaireTemplateListQueryDto query, CancellationToken cancellationToken = default)
        => await _queryHandler.GetFilteredAsync(query, cancellationToken);

    public async Task<QuestionnaireTemplateDto?> GetByIdAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _queryHandler.GetByIdNullableAsync(name, version, cancellationToken);
        }
        catch (ArgumentNullException ex)
        {
            LogError(ex, $"GetByIdAsync: Invalid parameters: Name={name}, Version={version}");
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetByIdAsync: Error retrieving template by id: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<QuestionnaireTemplateDto?> GetLatestVersionAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _queryHandler.GetLatestVersionAsync(name, cancellationToken);
        }
        catch (ArgumentNullException ex)
        {
            LogError(ex, $"GetLatestVersionAsync: Invalid parameter: Name={name}");
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetLatestVersionAsync: Error retrieving latest template version: Name={name}");
            throw;
        }
    }

    public async Task<IEnumerable<QuestionnaireTemplateDto>> GetAllVersionsAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _queryHandler.GetAllVersionsAsync(name, cancellationToken);
        }
        catch (ArgumentNullException ex)
        {
            LogError(ex, $"GetAllVersionsAsync: Invalid parameter: Name={name}");
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetAllVersionsAsync: Error retrieving all template versions: Name={name}");
            throw;
        }
    }

    public async Task<QuestionnaireTemplateDto> CreateAsync(QuestionnaireTemplateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        try
        {
            await ValidateDtoAsync(dto, cancellationToken);
            await EnsureTemplateNameDoesNotExistAsync(dto.Name, cancellationToken);

            dto.Version = InitialVersion;
            dto.Status = StatusDraft;
            dto.PublishedAt = null;

            var entity = MapToEntity(dto);
            await _templateRepository.AddAsync(entity, cancellationToken);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<QuestionnaireTemplateDto>(entity);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"CreateAsync: Error creating template: Name={dto.Name}");
            throw new InvalidOperationException($"Failed to create template '{dto.Name}'.", ex);
        }
    }

    public async Task<QuestionnaireTemplateDto> UpdateAsync(QuestionnaireTemplateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        try
        {
            await ValidateDtoAsync(dto, cancellationToken);

            if (dto.ShouldUpdateVersion == true)
            {
                return await VersionTemplateAsync(dto, cancellationToken);
            }

            return await UpdateTemplateInPlaceAsync(dto, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"UpdateAsync: Error updating template: Name={dto.Name}, Version={dto.Version}");
            throw new InvalidOperationException($"Failed to update template '{dto.Name}' v{dto.Version}.", ex);
        }
    }

    private async Task<QuestionnaireTemplateDto> VersionTemplateAsync(QuestionnaireTemplateDto dto, CancellationToken cancellationToken)
    {
        var exists = await _queryHandler.TemplateExistsAsync(dto.Name, dto.Version, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Template '{dto.Name}' v{dto.Version} not found.");

        var dummyExisting = new Domain.Models.QuestionnaireTemplate { Name = dto.Name, Version = dto.Version };
        return await _orchestrator.VersionTemplateAsync(dummyExisting, dto, cancellationToken);
    }

    private async Task<QuestionnaireTemplateDto> UpdateTemplateInPlaceAsync(QuestionnaireTemplateDto dto, CancellationToken cancellationToken)
    {
        var existing = await _queryHandler.GetTemplateForUpdateAsync(dto.Name, dto.Version, cancellationToken);
        if (existing == null)
            throw new InvalidOperationException($"Template '{dto.Name}' v{dto.Version} not found.");

        ValidateTemplateCanBeUpdated(existing, dto);

        var isInUse = await _queryHandler.IsTemplateInUseBySubmissionsAsync(dto.Name, dto.Version, cancellationToken);
        if (isInUse)
        {
            ValidateTemplateFieldsUnchanged(existing, dto);
        }
        else
        {
            UpdateTemplateFields(existing, dto);
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;

        var versionedTemplate = await _orchestrator.SyncSectionsAsync(
            existing, dto.Sections ?? new List<QuestionnaireSectionDto>(), cancellationToken);
        if (versionedTemplate != null)
            return versionedTemplate;

        await _templateRepository.SaveChangesAsync(cancellationToken);
        return _mapper.Map<QuestionnaireTemplateDto>(existing);
    }

    private static void ValidateTemplateCanBeUpdated(
        Domain.Models.QuestionnaireTemplate existing,
        QuestionnaireTemplateDto dto)
    {
        if (!string.Equals(existing.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Template name cannot be changed. The template name is '{existing.Name}' and cannot be modified. " +
                "If you need a different name, please create a new template.");
        }

        if (existing.Version != dto.Version)
        {
            throw new InvalidOperationException(
                $"Template version cannot be changed directly. Current version is {existing.Version}. " +
                "To create a new version, set 'ShouldUpdateVersion' to true.");
        }
    }

    private static void ValidateTemplateFieldsUnchanged(
        Domain.Models.QuestionnaireTemplate existing,
        QuestionnaireTemplateDto dto)
    {
        var errorMessage = "Template is in use. Template fields cannot be edited. Please create a new template version.";

        if (Enum.TryParse<Domain.Enums.QuestionnaireTemplateTypeEnum>(dto.TemplateType, true, out var incomingType) &&
            existing.TemplateType != incomingType)
            throw new InvalidOperationException(errorMessage);

        if (!string.Equals(existing.Title, dto.Title, StringComparison.Ordinal))
            throw new InvalidOperationException(errorMessage);

        if (!string.Equals(existing.Description, dto.Description, StringComparison.Ordinal))
            throw new InvalidOperationException(errorMessage);

        if (existing.TimeLimitSeconds != dto.TimeLimitSeconds)
            throw new InvalidOperationException(errorMessage);

        if (!string.Equals(existing.Status, dto.Status, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(errorMessage);
    }

    private static void UpdateTemplateFields(
        Domain.Models.QuestionnaireTemplate existing,
        QuestionnaireTemplateDto dto)
    {
        if (Enum.TryParse<Domain.Enums.QuestionnaireTemplateTypeEnum>(dto.TemplateType, true, out var templateType))
            existing.TemplateType = templateType;
        existing.Status = dto.Status;
        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.TimeLimitSeconds = dto.TimeLimitSeconds;
    }

    public async Task<QuestionnaireTemplateDto> DuplicateTemplateAsync(
        string sourceName, 
        int sourceVersion, 
        DuplicateTemplateRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.NewName)) 
            throw new InvalidOperationException("New template name is required.");

        try
        {
            var source = await _queryHandler.GetTemplateForDuplicateAsync(sourceName, sourceVersion, cancellationToken);
            if (source == null)
                throw new InvalidOperationException($"Template '{sourceName}' v{sourceVersion} not found.");

            var newTemplate = CreateDuplicateTemplate(source, request);

            if (request.IncludeQuestions)
            {
                DuplicateSectionsAndQuestions(source, newTemplate);
            }

            await _templateRepository.AddAsync(newTemplate, cancellationToken);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<QuestionnaireTemplateDto>(newTemplate);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"DuplicateTemplateAsync: Error duplicating template: SourceName={sourceName}, SourceVersion={sourceVersion}");
            throw new InvalidOperationException($"Failed to duplicate template '{sourceName}' v{sourceVersion}.", ex);
        }
    }

    private static Domain.Models.QuestionnaireTemplate CreateDuplicateTemplate(
        Domain.Models.QuestionnaireTemplate source,
        DuplicateTemplateRequestDto request)
    {
        return new Domain.Models.QuestionnaireTemplate
        {
            Name = request.NewName.Trim(),
            Version = InitialVersion,
            TemplateType = source.TemplateType,
            Status = StatusDraft,
            Title = request.IncludeTitle ? source.Title : null,
            Description = request.IncludeDescription ? source.Description : null,
            TimeLimitSeconds = source.TimeLimitSeconds,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Sections = new List<Domain.Models.QuestionnaireSection>()
        };
    }

    private static void DuplicateSectionsAndQuestions(
        Domain.Models.QuestionnaireTemplate source,
        Domain.Models.QuestionnaireTemplate newTemplate)
    {
        foreach (var section in source.Sections)
        {
            var newSection = new Domain.Models.QuestionnaireSection
            {
                Id = Guid.NewGuid(),
                QuestionnaireTemplateName = newTemplate.Name,
                QuestionnaireTemplateVersion = newTemplate.Version,
                Order = section.Order,
                Title = section.Title,
                Description = section.Description,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Questions = new List<Domain.Models.QuestionnaireQuestion>()
            };

            foreach (var question in section.Questions)
            {
                var newQuestion = CreateQuestionForDuplicate(question, newSection.Id);
                newSection.Questions.Add(newQuestion);
            }

            newTemplate.Sections.Add(newSection);
        }
    }

    public async Task<List<VersionHistoryItemDto>> GetVersionHistoryAsync(
        string name,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (entityType.Equals(EntityTypeTemplate, StringComparison.OrdinalIgnoreCase))
            {
                return await _queryHandler.GetTemplateVersionHistoryAsync(name, cancellationToken);
            }

            return await _versioningService.GetVersionHistoryAsync(name, entityType, cancellationToken);
        }
        catch (ArgumentNullException ex)
        {
            LogError(ex, $"GetVersionHistoryAsync: Invalid parameters: Name={name}, EntityType={entityType}");
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetVersionHistoryAsync: Error retrieving version history: Name={name}, EntityType={entityType}");
            throw;
        }
    }

    public async Task<QuestionnaireTemplateDto> SetActiveQuestionVersionAsync(
        string templateName,
        int templateVersion,
        int sectionOrder,
        string questionName,
        int questionVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateActiveQuestionVersionParameters(templateName, questionName, templateVersion, sectionOrder, questionVersion);

            var isInUse = await _queryHandler.IsTemplateInUseBySubmissionsAsync(templateName, templateVersion, cancellationToken);
            if (isInUse)
            {
                throw new InvalidOperationException(
                    "Template is in use. Changing the active question version is not allowed. Please create a new template version instead.");
            }

            var (template, section, target) = await GetTemplateSectionAndQuestionAsync(
                templateName, templateVersion, sectionOrder, questionName, questionVersion, cancellationToken);

            await ActivateQuestionVersionAsync(section, target, cancellationToken);

            var refreshed = await _queryHandler.GetTemplateForUpdateAsync(templateName, templateVersion, cancellationToken);
            if (refreshed == null)
                throw new InvalidOperationException($"Template '{templateName}' v{templateVersion} not found after activation.");

            return _mapper.Map<QuestionnaireTemplateDto>(refreshed);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"SetActiveQuestionVersionAsync: Error setting active question version: TemplateName={templateName}, TemplateVersion={templateVersion}, QuestionName={questionName}, QuestionVersion={questionVersion}");
            throw new InvalidOperationException($"Failed to set active question version for '{questionName}' v{questionVersion}.", ex);
        }
    }

    private static void ValidateActiveQuestionVersionParameters(
        string templateName,
        string questionName,
        int templateVersion,
        int sectionOrder,
        int questionVersion)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentNullException(nameof(templateName));
        if (string.IsNullOrWhiteSpace(questionName))
            throw new ArgumentNullException(nameof(questionName));
        if (templateVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(templateVersion));
        if (sectionOrder <= 0)
            throw new ArgumentOutOfRangeException(nameof(sectionOrder));
        if (questionVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(questionVersion));
    }

    private async Task<(Domain.Models.QuestionnaireTemplate Template, Domain.Models.QuestionnaireSection Section, Domain.Models.QuestionnaireQuestion Target)> GetTemplateSectionAndQuestionAsync(
        string templateName,
        int templateVersion,
        int sectionOrder,
        string questionName,
        int questionVersion,
        CancellationToken cancellationToken)
    {
        var template = await _queryHandler.GetTemplateForQuestionActivationAsync(
            templateName, templateVersion, sectionOrder, cancellationToken);

        if (template == null)
            throw new InvalidOperationException($"Template '{templateName}' v{templateVersion} not found.");

        var section = template.Sections.FirstOrDefault(s => s.Order == sectionOrder);
        if (section == null)
            throw new InvalidOperationException($"Section order {sectionOrder} not found in template '{templateName}' v{templateVersion}.");

        var target = FindQuestionInSection(section, questionName, questionVersion, sectionOrder);
        return (template, section, target);
    }

    private static Domain.Models.QuestionnaireQuestion FindQuestionInSection(
        Domain.Models.QuestionnaireSection section,
        string questionName,
        int questionVersion,
        int sectionOrder)
    {
        var candidates = section.Questions
            .Where(q => !q.IsDeleted && string.Equals(q.Name, questionName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0)
            throw new InvalidOperationException($"Question '{questionName}' not found in section {sectionOrder}.");

        var target = candidates.FirstOrDefault(q => q.Version == questionVersion);
        if (target == null)
            throw new InvalidOperationException($"Question '{questionName}' v{questionVersion} not found in section {sectionOrder}.");

        return target;
    }

    private async Task ActivateQuestionVersionAsync(
        Domain.Models.QuestionnaireSection section,
        Domain.Models.QuestionnaireQuestion target,
        CancellationToken cancellationToken)
    {
        var candidates = section.Questions
            .Where(q => !q.IsDeleted && string.Equals(q.Name, target.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var slotQuestions = section.Questions
            .Where(q => !q.IsDeleted && q.Order == target.Order)
            .ToList();

        var toDeactivate = candidates
            .Concat(slotQuestions)
            .Distinct()
            .ToList();

        var previouslyActive = toDeactivate.Where(q => q.IsActive).ToList();

        try
        {
            DeactivateQuestions(toDeactivate);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            if (!target.IsActive)
            {
                target.IsActive = true;
                target.UpdatedAt = DateTimeOffset.UtcNow;
                await _templateRepository.SaveChangesAsync(cancellationToken);
            }
        }
        catch
        {
            RestoreActiveQuestions(previouslyActive);
            try { await _templateRepository.SaveChangesAsync(cancellationToken); } catch { }
            throw;
        }
    }

    private static void DeactivateQuestions(List<Domain.Models.QuestionnaireQuestion> questions)
    {
        foreach (var question in questions.Where(q => q.IsActive))
        {
            question.IsActive = false;
            question.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    private static void RestoreActiveQuestions(List<Domain.Models.QuestionnaireQuestion> questions)
    {
        foreach (var question in questions)
        {
            question.IsActive = true;
            question.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public async Task<QuestionnaireQuestionDto> GetQuestionVersionAsync(
        string questionName,
        int questionVersion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(questionName))
            throw new ArgumentNullException(nameof(questionName));
        if (questionVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(questionVersion));

        try
        {
            var dto = await _queryHandler.GetQuestionVersionAsync(questionName, questionVersion, cancellationToken);
            if (dto == null)
                throw new InvalidOperationException($"Question '{questionName}' v{questionVersion} not found.");

            return dto;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetQuestionVersionAsync: Error retrieving question version: Name={questionName}, Version={questionVersion}");
            throw;
        }
    }

    public async Task<List<QuestionnaireQuestionHistoryDetailsDto>> GetQuestionHistoryWithOptionsAsync(
        string questionName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(questionName))
            throw new ArgumentNullException(nameof(questionName));

        try
        {
            return await _queryHandler.GetQuestionHistoryWithOptionsAsync(questionName, cancellationToken);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetQuestionHistoryWithOptionsAsync: Error retrieving question history: Name={questionName}");
            throw;
        }
    }

    public async Task<Result<QuestionnaireTemplateDeleteResultDto>> DeleteAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _queryHandler.GetTemplateForDeleteAsync(name, version, cancellationToken);
            if (entity == null)
                return Result<QuestionnaireTemplateDeleteResultDto>.NotFound();

            var inUseByJobSteps = await _queryHandler.IsTemplateInUseByJobStepsAsync(name, version, cancellationToken);
            var inUseBySubmissions = await _queryHandler.IsTemplateInUseBySubmissionsAsync(name, version, cancellationToken);

            if (inUseByJobSteps || inUseBySubmissions)
            {
                return await SoftDeleteTemplateAsync(entity, cancellationToken);
            }

            return await HardDeleteTemplateAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"DeleteAsync: Error deleting template: Name={name}, Version={version}");
            return Result<QuestionnaireTemplateDeleteResultDto>.Error($"Failed to delete template '{name}' v{version}.");
        }
    }

    private async Task<Result<QuestionnaireTemplateDeleteResultDto>> SoftDeleteTemplateAsync(
        Domain.Models.QuestionnaireTemplate entity,
        CancellationToken cancellationToken)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _templateRepository.UpdateAsync(entity, cancellationToken);
        await _templateRepository.SaveChangesAsync(cancellationToken);
        return Result.Success(new QuestionnaireTemplateDeleteResultDto { Mode = "Archived" });
    }

    private async Task<Result<QuestionnaireTemplateDeleteResultDto>> HardDeleteTemplateAsync(
        Domain.Models.QuestionnaireTemplate entity,
        CancellationToken cancellationToken)
    {
        await _templateRepository.DeleteAsync(entity, cancellationToken);
        await _templateRepository.SaveChangesAsync(cancellationToken);
        return Result.Success(new QuestionnaireTemplateDeleteResultDto { Mode = "Deleted" });
    }

    public async Task<Result> RestoreAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _queryHandler.GetTemplateForRestoreAsync(name, version, cancellationToken);
            if (entity == null)
                return Result.NotFound();

            if (!entity.IsDeleted)
                return Result.Success();

            entity.IsDeleted = false;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _templateRepository.UpdateAsync(entity, cancellationToken);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(ex, $"RestoreAsync: Error restoring template: Name={name}, Version={version}");
            return Result.Error($"Failed to restore template '{name}' v{version}.");
        }
    }

    public async Task<Result> PublishAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _queryHandler.GetTemplateForPublishAsync(name, version, cancellationToken);
            if (entity == null)
                return Result.NotFound();

            if (string.Equals(entity.Status, StatusPublished, StringComparison.OrdinalIgnoreCase))
                return Result.Success();

            entity.Status = StatusPublished;
            entity.PublishedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _templateRepository.UpdateAsync(entity, cancellationToken);
            await _templateRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogError(ex, $"PublishAsync: Error publishing template: Name={name}, Version={version}");
            return Result.Error($"Failed to publish template '{name}' v{version}.");
        }
    }

    public async Task<CandidateQuestionnaireTemplateDto?> GetCandidateTemplateAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _queryHandler.GetTemplateForCandidateAsync(name, version, cancellationToken);
            return entity != null ? _mapper.Map<CandidateQuestionnaireTemplateDto>(entity) : null;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetCandidateTemplateAsync: Error retrieving candidate template: Name={name}, Version={version}");
            throw;
        }
    }

    private async Task ValidateDtoAsync(QuestionnaireTemplateDto dto, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException(errors);
        }
    }

    private async Task EnsureTemplateNameDoesNotExistAsync(string templateName, CancellationToken cancellationToken)
    {
        var templateExists = await _queryHandler.TemplateNameExistsAsync(templateName, cancellationToken);
        if (templateExists)
        {
            throw new InvalidOperationException(
                $"A questionnaire template with name '{templateName}' already exists. " +
                "Please use a different name or use UpdateAsync to create a new version.");
        }
    }

    private static Domain.Models.QuestionnaireQuestion CreateQuestionForDuplicate(
        Domain.Models.QuestionnaireQuestion source,
        Guid sectionId)
    {
        var newQuestion = new Domain.Models.QuestionnaireQuestion
        {
            Name = Guid.NewGuid().ToString(),
            Version = InitialVersion,
            QuestionnaireSectionId = sectionId,
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
            Options = new List<Domain.Models.QuestionnaireQuestionOption>()
        };

        foreach (var option in source.Options)
        {
            newQuestion.Options.Add(new Domain.Models.QuestionnaireQuestionOption
            {
                Name = Guid.NewGuid().ToString(),
                Version = InitialVersion,
                QuestionnaireQuestionName = newQuestion.Name,
                QuestionnaireQuestionVersion = newQuestion.Version,
                Order = option.Order,
                Label = option.Label,
                MediaFileId = option.MediaFileId,
                MediaUrl = option.MediaUrl,
                IsCorrect = option.IsCorrect,
                Score = option.Score,
                Weight = option.Weight,
                Wa = option.Wa,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        return newQuestion;
    }

    private Domain.Models.QuestionnaireTemplate MapToEntity(QuestionnaireTemplateDto dto)
    {
        var template = new Domain.Models.QuestionnaireTemplate
        {
            Name = dto.Name.Trim(),
            Version = dto.Version,
            TemplateType = Enum.TryParse<Domain.Enums.QuestionnaireTemplateTypeEnum>(dto.TemplateType, true, out var templateType) ? templateType : Domain.Enums.QuestionnaireTemplateTypeEnum.Form,
            Status = dto.Status,
            Title = dto.Title,
            Description = dto.Description,
            TimeLimitSeconds = dto.TimeLimitSeconds,
            PublishedAt = dto.PublishedAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Sections = new List<Domain.Models.QuestionnaireSection>()
        };

        foreach (var s in dto.Sections ?? new List<QuestionnaireSectionDto>())
        {
            var section = new Domain.Models.QuestionnaireSection
            {
                Id = s.Id,
                QuestionnaireTemplateName = template.Name,
                QuestionnaireTemplateVersion = template.Version,
                Order = s.Order,
                Title = s.Title?.Trim() ?? string.Empty,
                Description = s.Description,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Questions = new List<Domain.Models.QuestionnaireQuestion>()
            };

            foreach (var q in s.Questions ?? new List<QuestionnaireQuestionDto>())
            {
                var questionText = q.PromptText?.Trim() ?? string.Empty;
                var questionName = string.IsNullOrWhiteSpace(q.Name)
                    ? GenerateQuestionName(template.Name, template.Version, questionText)
                    : q.Name;

                var questionDto = new QuestionnaireQuestionDto
                {
                    Name = questionName,
                    Version = InitialVersion,
                    Order = q.Order,
                    QuestionType = q.QuestionType,
                    PromptText = questionText,
                    IsRequired = q.IsRequired,
                    TraitKey = q.TraitKey,
                    Ws = q.Ws,
                    MediaUrl = q.MediaUrl,
                    MediaFileId = q.MediaFileId,
                    Options = q.Options ?? new List<QuestionnaireOptionDto>()
                };

                var question = _entityFactory.CreateQuestion(questionDto, section.Id);

                foreach (var o in q.Options ?? new List<QuestionnaireOptionDto>())
                {
                    var option = _entityFactory.CreateOption(o, question);
                    question.Options.Add(option);
                }

                section.Questions.Add(question);
            }

            template.Sections.Add(section);
        }

        return template;
    }

    private static string GenerateQuestionName(string templateName, int templateVersion, string questionText)
    {
        var templateNameSlug = StringHelper.Slugify(templateName);
        var questionTextSlug = StringHelper.Slugify(questionText);
        return $"{templateNameSlug}_v{templateVersion}_{questionTextSlug}";
    }

    private void LogError(Exception ex, string message)
    {
        _logger.LogError(ex, message);
    }
}


