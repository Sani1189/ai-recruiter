using Ardalis.Result;
using Recruiter.Application.QuestionnaireTemplate.Dto;

namespace Recruiter.Application.QuestionnaireTemplate.Interfaces;

public interface IQuestionnaireTemplateService
{
    Task<Result<List<QuestionnaireTemplateDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Recruiter.Application.Common.Dto.PagedResult<QuestionnaireTemplateDto>>> GetFilteredAsync(QuestionnaireTemplateListQueryDto query, CancellationToken cancellationToken = default);
    Task<QuestionnaireTemplateDto?> GetByIdAsync(string name, int version, CancellationToken cancellationToken = default);
    Task<QuestionnaireTemplateDto?> GetLatestVersionAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireTemplateDto>> GetAllVersionsAsync(string name, CancellationToken cancellationToken = default);

    Task<QuestionnaireTemplateDto> CreateAsync(QuestionnaireTemplateDto dto, CancellationToken cancellationToken = default);
    Task<QuestionnaireTemplateDto> UpdateAsync(QuestionnaireTemplateDto dto, CancellationToken cancellationToken = default);
    Task<Result<QuestionnaireTemplateDeleteResultDto>> DeleteAsync(string name, int version, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(string name, int version, CancellationToken cancellationToken = default);

    Task<Result> PublishAsync(string name, int version, CancellationToken cancellationToken = default);

    Task<CandidateQuestionnaireTemplateDto?> GetCandidateTemplateAsync(string name, int version, CancellationToken cancellationToken = default);
    
    Task<QuestionnaireTemplateDto> DuplicateTemplateAsync(
        string sourceName, 
        int sourceVersion, 
        DuplicateTemplateRequestDto request, 
        CancellationToken cancellationToken = default);
    
    Task<List<VersionHistoryItemDto>> GetVersionHistoryAsync(
        string name,
        string entityType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets which question version is active (visible in the editable template read model) for a given
    /// template version + section order + question name.
    /// Old versions remain stored for history; only one version can be active per (SectionId, Order).
    /// </summary>
    Task<QuestionnaireTemplateDto> SetActiveQuestionVersionAsync(
        string templateName,
        int templateVersion,
        int sectionOrder,
        string questionName,
        int questionVersion,
        CancellationToken cancellationToken = default);

    Task<QuestionnaireQuestionDto> GetQuestionVersionAsync(
        string questionName,
        int questionVersion,
        CancellationToken cancellationToken = default);

    Task<List<QuestionnaireQuestionHistoryDetailsDto>> GetQuestionHistoryWithOptionsAsync(
        string questionName,
        CancellationToken cancellationToken = default);
}


