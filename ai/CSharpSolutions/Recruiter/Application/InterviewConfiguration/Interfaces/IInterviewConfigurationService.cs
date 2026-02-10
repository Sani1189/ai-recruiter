using Recruiter.Application.InterviewConfiguration.Dto;
using Ardalis.Result;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.InterviewConfiguration.Interfaces;

public interface IInterviewConfigurationService
{
    Task<IEnumerable<InterviewConfigurationDto>> GetAllAsync();
    Task<InterviewConfigurationDto?> GetByIdAsync(string name, int version);
    Task<InterviewConfigurationDto?> GetLatestVersionAsync(string name);
    Task<IEnumerable<InterviewConfigurationDto>> GetAllVersionsAsync(string name);
    Task<InterviewConfigurationDto> CreateAsync(InterviewConfigurationDto dto);
    Task<InterviewConfigurationDto> UpdateAsync(InterviewConfigurationDto dto);
    Task DeleteAsync(string name, int version);
    Task<bool> ExistsAsync(string name, int version);
    Task<Result<InterviewConfigurationDto>> GetLatestByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<List<InterviewConfigurationDto>>> GetByModalityAsync(string modality, CancellationToken cancellationToken = default);
    Task<Result<List<InterviewConfigurationDto>>> GetActiveConfigurationsAsync(CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<InterviewConfigurationDto>>> GetFilteredConfigurationsAsync(InterviewConfigurationListQueryDto query, CancellationToken cancellationToken = default);

    Task<InterviewConfigurationDto?> DuplicateAsync(string sourceName, int sourceVersion, DuplicateInterviewConfigurationRequestDto request);
    
    // New methods for prompt resolution and validation
    Task<InterviewConfigurationWithPromptsDto?> GetWithResolvedPromptsAsync(string name, int version);
    Task<InterviewConfigurationWithPromptsDto?> GetLatestWithResolvedPromptsAsync(string name);
    Task<List<PromptVersionDto>> GetPromptVersionsAsync(string promptName);
    Task<bool> ValidatePromptAsync(string promptName, int? version = null);
}
