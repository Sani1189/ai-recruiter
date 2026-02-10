using Ardalis.Result;
using FluentValidation;
using Recruiter.Application.UserProfile.Dto;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.Application.UserProfile;

// UserProfile Orchestrator for complex business operations
public interface IUserProfileOrchestrator
{
    Task<Result<UserProfileDto>> GetUserProfileWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> ProcessUserProfileAsync(UserProfileDto userProfileDto, CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> UpdateUserRoleAsync(Guid id, string role, CancellationToken cancellationToken = default);
}

// UserProfile Orchestrator implementation
public class UserProfileOrchestrator : IUserProfileOrchestrator
{
    private readonly IUserProfileService _userProfileService;
    private readonly IValidator<UserProfileDto> _userProfileValidator;

    public UserProfileOrchestrator(
        IUserProfileService userProfileService,
        IValidator<UserProfileDto> userProfileValidator)
    {
        _userProfileService = userProfileService;
        _userProfileValidator = userProfileValidator;
    }

    public async Task<Result<UserProfileDto>> GetUserProfileWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userProfile = await _userProfileService.GetByIdAsync(id);
        if (userProfile == null)
        {
            return Result<UserProfileDto>.NotFound($"User profile with ID {id} not found");
        }

        // Get related details would be handled here
        // This is a simple implementation - in real scenario you might want to include additional details

        return Result<UserProfileDto>.Success(userProfile);
    }

    public async Task<Result<UserProfileDto>> ProcessUserProfileAsync(UserProfileDto userProfileDto, CancellationToken cancellationToken = default)
    {
        // Validate the user profile
        var validationResult = await _userProfileValidator.ValidateAsync(userProfileDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<UserProfileDto>.Invalid(validationResult.Errors.Select(e => new ValidationError { ErrorMessage = e.ErrorMessage }).ToArray());
        }

        // Process the user profile (create or update)
        UserProfileDto result;
        if (userProfileDto.Id == Guid.Empty)
        {
            result = await _userProfileService.CreateAsync(userProfileDto);
        }
        else
        {
            result = await _userProfileService.UpdateAsync(userProfileDto);
        }

        return Result<UserProfileDto>.Success(result);
    }

    public async Task<Result<UserProfileDto>> UpdateUserRoleAsync(Guid id, string role, CancellationToken cancellationToken = default)
    {
        var userProfile = await _userProfileService.GetByIdAsync(id);
        if (userProfile == null)
        {
            return Result<UserProfileDto>.NotFound($"User profile with ID {id} not found");
        }

        // Update nationality (closest to role concept)
        userProfile.Nationality = role;
        var updatedUserProfile = await _userProfileService.UpdateAsync(userProfile);

        return Result<UserProfileDto>.Success(updatedUserProfile);
    }
}
