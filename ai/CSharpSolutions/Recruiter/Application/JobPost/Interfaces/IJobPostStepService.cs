using Recruiter.Application.Common.Dto;
using Recruiter.Application.JobPost.Dto;

namespace Recruiter.Application.JobPost.Interfaces;

public interface IJobPostStepService
{
    Task<JobPostStepDto?> GetByIdAsync(string name, int version);
    Task<JobPostStepDto?> GetLatestVersionAsync(string name);
    Task<PagedResult<JobPostStepDto>> GetListAsync(JobPostStepQueryDto query);
    Task<IEnumerable<JobPostStepDto>> GetDropdownListAsync();
    Task<IEnumerable<JobStepVersionDto>> GetAllVersionsAsync(string name);
    Task<JobPostStepDto> CreateAsync(JobPostStepDto dto);
    Task<JobPostStepDto> UpdateAsync(JobPostStepDto dto);
    Task<JobPostStepDto?> DuplicateAsync(string sourceName, int sourceVersion, DuplicateJobPostStepRequestDto request);
    Task DeleteAsync(string name, int version);
    Task<bool> ExistsAsync(string name, int version);
}