using Recruiter.Application.Prompt.Dto;
using Ardalis.Result;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Prompt.Interfaces;

public interface IPromptService
{
    Task<Result<List<PromptDto>>> GetAllAsync();
    Task<Result<List<string>>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<PromptDto?> GetByIdAsync(string name, int version);
    Task<PromptDto?> GetLatestVersionAsync(string name);
    Task<IEnumerable<PromptDto>> GetAllVersionsAsync(string name);
    Task<PromptDto> CreateAsync(PromptDto dto);
    Task<PromptDto> UpdateAsync(PromptDto dto);
    Task<PromptDto?> DuplicateAsync(string sourceName, int sourceVersion, DuplicatePromptRequestDto request);
    Task<Result> DeleteAsync(string name, int version);
    Task<bool> ExistsAsync(string name, int version);
    Task<Result<PromptDto>> GetLatestByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<List<PromptDto>>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<Result<List<PromptDto>>> GetByLocaleAsync(string locale, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<PromptDto>>> GetFilteredPromptsAsync(PromptListQueryDto query, CancellationToken cancellationToken = default);
}
