using Recruiter.Application.UserProfile.Dto;
using Ardalis.Result;
using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.UserProfile.Interfaces;

public interface IUserProfileService
{
    Task<IEnumerable<UserProfileDto>> GetAllAsync();
    Task<UserProfileDto?> GetByIdAsync(Guid id);
    Task<UserProfileDto> CreateAsync(UserProfileDto dto);
    Task<UserProfileDto> UpdateAsync(UserProfileDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<UserProfileDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<List<UserProfileDto>>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<List<UserProfileDto>>> GetByNationalityAsync(string nationality, CancellationToken cancellationToken = default);
    Task<Result<List<UserProfileDto>>> GetByAgeRangeAsync(int minAge, int maxAge, CancellationToken cancellationToken = default);
    Task<Result<Common.Dto.PagedResult<UserProfileDto>>> GetFilteredUserProfilesAsync(UserProfileListQueryDto query, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateCurrentUserProfileAsync(UserProfileDto userProfileDto);
    Task<Result<UserProfileDetailsDto>> GetUserProfileDetailsAsync(Guid userId);
}
