using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Interfaces;

public interface IQuestionnaireVersioningService
{
    Task<QuestionnaireQuestion> VersionQuestionAsync(
        QuestionnaireQuestion source, 
        Guid newSectionId, 
        CancellationToken cancellationToken = default);
    
    Task<QuestionnaireQuestionOption> VersionOptionAsync(
        QuestionnaireQuestionOption source, 
        string newQuestionName, 
        int newQuestionVersion, 
        CancellationToken cancellationToken = default);
    
    Task<Dictionary<string, int>> GetLatestQuestionVersionsAsync(
        IEnumerable<string> questionNames, 
        CancellationToken cancellationToken = default);
    
    Task<Dictionary<string, int>> GetLatestOptionVersionsAsync(
        IEnumerable<string> optionNames, 
        CancellationToken cancellationToken = default);

    Task<Domain.Models.QuestionnaireTemplate> ResolveToLatestVersionsAsync(
        Domain.Models.QuestionnaireTemplate template,
        CancellationToken cancellationToken = default);
    
    Task<List<VersionHistoryItemDto>> GetVersionHistoryAsync(
        string name,
        string entityType,
        CancellationToken cancellationToken = default);
}
