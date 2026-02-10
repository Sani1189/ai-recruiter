using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Domain.Models;
using DomainQuestionnaireTemplate = Recruiter.Domain.Models.QuestionnaireTemplate;

namespace Recruiter.Application.QuestionnaireTemplate.Handlers;

/// <summary>
/// Handles synchronization of questionnaire sections.
/// </summary>
public sealed class SectionSyncHandler
{
    private readonly IRepository<QuestionnaireSection> _sectionRepository;
    private readonly IRepository<QuestionnaireCandidateSubmission> _submissionRepository;
    private readonly QuestionSyncHandler _questionSyncHandler;

    public SectionSyncHandler(
        IRepository<QuestionnaireSection> sectionRepository,
        IRepository<QuestionnaireCandidateSubmission> submissionRepository,
        QuestionSyncHandler questionSyncHandler)
    {
        _sectionRepository = sectionRepository ?? throw new ArgumentNullException(nameof(sectionRepository));
        _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        _questionSyncHandler = questionSyncHandler ?? throw new ArgumentNullException(nameof(questionSyncHandler));
    }

    public async Task<QuestionnaireTemplateDto?> SyncSectionsAsync(
        DomainQuestionnaireTemplate existing,
        List<QuestionnaireSectionDto> incoming,
        CancellationToken cancellationToken)
    {
        var isTemplateInUse = await IsTemplateInUseAsync(existing.Name, existing.Version, cancellationToken);

        var incomingByOrder = incoming.ToDictionary(x => x.Order, x => x);
        var existingByOrder = existing.Sections.ToDictionary(x => x.Order, x => x);

        await RemoveSectionsNotInIncomingAsync(existing, incomingByOrder, isTemplateInUse, cancellationToken);

        foreach (var sectionDto in incoming)
        {
            var result = await ProcessSectionAsync(
                sectionDto, existing, existingByOrder, isTemplateInUse, cancellationToken);

            if (result != null)
                return result;
        }

        return null;
    }

    private async Task RemoveSectionsNotInIncomingAsync(
        DomainQuestionnaireTemplate existing,
        Dictionary<int, QuestionnaireSectionDto> incomingByOrder,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        var sectionsToRemove = existing.Sections
            .Where(s => !incomingByOrder.ContainsKey(s.Order))
            .ToList();

        foreach (var section in sectionsToRemove)
        {
            if (isTemplateInUse)
            {
                throw new InvalidOperationException(
                    "Cannot remove section. Template is in use. Please version the template first.");
            }
            existing.Sections.Remove(section);
        }
    }

    private async Task<QuestionnaireTemplateDto?> ProcessSectionAsync(
        QuestionnaireSectionDto sectionDto,
        DomainQuestionnaireTemplate template,
        Dictionary<int, QuestionnaireSection> existingByOrder,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        if (!existingByOrder.TryGetValue(sectionDto.Order, out var section) || section == null)
        {
            return await CreateNewSectionAsync(sectionDto, template, isTemplateInUse, cancellationToken);
        }

        return await UpdateExistingSectionAsync(section, sectionDto, template, isTemplateInUse, cancellationToken);
    }

    private async Task<QuestionnaireTemplateDto?> CreateNewSectionAsync(
        QuestionnaireSectionDto sectionDto,
        DomainQuestionnaireTemplate template,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        if (isTemplateInUse)
        {
            throw new InvalidOperationException(
                "Cannot add section. Template is in use. Please version the template first.");
        }

        var newSection = new QuestionnaireSection
        {
            Id = sectionDto.Id != Guid.Empty ? sectionDto.Id : Guid.NewGuid(),
            QuestionnaireTemplateName = template.Name,
            QuestionnaireTemplateVersion = template.Version,
            Order = sectionDto.Order,
            Title = sectionDto.Title?.Trim() ?? string.Empty,
            Description = sectionDto.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Questions = new List<QuestionnaireQuestion>()
        };

        await _sectionRepository.AddAsync(newSection, cancellationToken);
        template.Sections.Add(newSection);

        var questionResult = await _questionSyncHandler.SyncQuestionsAsync(
            newSection,
            sectionDto.Questions ?? new List<QuestionnaireQuestionDto>(),
            template,
            cancellationToken);
        return questionResult.VersionedTemplate;
    }

    private async Task<QuestionnaireTemplateDto?> UpdateExistingSectionAsync(
        QuestionnaireSection section,
        QuestionnaireSectionDto sectionDto,
        DomainQuestionnaireTemplate template,
        bool isTemplateInUse,
        CancellationToken cancellationToken)
    {
        if (isTemplateInUse)
        {
            var incomingTitle = sectionDto.Title?.Trim() ?? string.Empty;
            if (!string.Equals(section.Title, incomingTitle, StringComparison.Ordinal) ||
                !string.Equals(section.Description, sectionDto.Description, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Cannot edit section. Template is in use. Please version the template first.");
            }
        }

        section.Title = sectionDto.Title?.Trim() ?? string.Empty;
        section.Description = sectionDto.Description;
        section.UpdatedAt = DateTimeOffset.UtcNow;

        var questionResult = await _questionSyncHandler.SyncQuestionsAsync(
            section,
            sectionDto.Questions ?? new List<QuestionnaireQuestionDto>(),
            template,
            cancellationToken);
        return questionResult.VersionedTemplate;
    }

    private async Task<bool> IsTemplateInUseAsync(string name, int version, CancellationToken cancellationToken)
    {
        var submissionCount = await _submissionRepository.CountAsync(
            new TemplateInUseBySubmissionsSpec(name, version), cancellationToken);
        return submissionCount > 0;
    }
}
