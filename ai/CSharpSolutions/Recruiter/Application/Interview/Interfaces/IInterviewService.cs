using Recruiter.Application.Interview.Dto;
using Ardalis.Result;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.Interview.Interfaces;

public interface IInterviewService
{
    Task<IEnumerable<InterviewDto>> GetAllAsync();
    Task<InterviewDto?> GetByIdAsync(Guid id);
    Task<InterviewDto> CreateAsync(InterviewDto dto);
    Task<InterviewDto> UpdateAsync(InterviewDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<InterviewDto>>> GetByJobApplicationStepIdAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default);
    Task<Result<List<InterviewDto>>> GetByConfigurationAsync(string configName, int configVersion, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<InterviewDto>>> GetFilteredInterviewsAsync(InterviewListQueryDto query, CancellationToken cancellationToken = default);
    Task<Result<InterviewDto>> CompleteInterviewAsync(Guid interviewId, CompleteInterviewDto completeDto, CancellationToken cancellationToken = default);
    Task<Result<InterviewDto>> CreateForStepAsync(Guid jobApplicationStepId, CancellationToken cancellationToken = default);
}
