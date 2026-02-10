using Recruiter.Application.QuestionnaireTemplate.Import.Dto;

namespace Recruiter.Application.QuestionnaireTemplate.Import.Interfaces;

public interface IQuestionnaireTemplateImportService
{
    Task<QuestionnaireTemplateImportValidationResultDto> ValidateAsync(
        Stream spreadsheetStream,
        QuestionnaireTemplateImportRequestDto? request = null,
        CancellationToken cancellationToken = default);

    Task<QuestionnaireTemplateImportExecuteResultDto> ExecuteAsync(
        Stream spreadsheetStream,
        QuestionnaireTemplateImportRequestDto? request = null,
        CancellationToken cancellationToken = default);
}

