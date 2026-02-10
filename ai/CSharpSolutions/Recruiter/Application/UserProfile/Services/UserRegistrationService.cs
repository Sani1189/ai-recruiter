using System.Security.Claims;
using AutoMapper;
using Recruiter.Application.Candidate.Dto;
using Recruiter.Application.Candidate.Interfaces;
using Recruiter.Application.Candidate.Specifications;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.UserProfile.Dto;
using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Application.UserProfile.Specifications;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Services;
public class UserRegistrationService : IUserRegistrationService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ICandidateService _candidateService;
    private readonly ICandidateRepository _candidateRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UserRegistrationService(
        IUserProfileRepository userProfileRepository,
        ICandidateService candidateService,
        ICandidateRepository candidateRepository,
        ICurrentUserService currentUserService,
        
        IMapper mapper)
    {
        _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
        _candidateService = candidateService ?? throw new ArgumentNullException(nameof(candidateService));
        _candidateRepository = candidateRepository ?? throw new ArgumentNullException(nameof(candidateRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserProfileDto> RegisterUserAsync(ClaimsPrincipal user)
    {
        var userInfo = _currentUserService.GetCurrentUserInfo();
        return await RegisterUserAsync(userInfo);
    }

     private async Task<UserProfileDto> RegisterUserAsync(UserInfoDto userInfo)
    {
        // Normalize email to lowercase for consistency
        var normalizedEmail = userInfo.Email?.ToLowerInvariant() ?? string.Empty;
        
        var existingUser = await GetExistingUserAsync(normalizedEmail);
        if (existingUser != null)
        {
            // Check if candidate exists for this user, if not create one
            await EnsureCandidateExistsAsync(existingUser.Id!.Value);
            return existingUser;
        }

        var newUser = new Domain.Models.UserProfile
        {
            Name = userInfo.Name,
            Email = normalizedEmail,
            PhoneNumber = null,
            Age = null,
            Nationality = null,
            ProfilePictureUrl = null,
            Bio = null,
            JobTypePreferences = new List<string>(),
            OpenToRelocation = false, // Explicitly set to false - EF Core will include this in INSERT
            RemotePreferences = new List<string>(),
            Roles = userInfo.Roles ?? new List<string>{ "Candidate" }
        };

        await _userProfileRepository.AddAsync(newUser);
        await _userProfileRepository.SaveChangesAsync();

        // Create candidate using the service to ensure CandidateId is generated and all business logic is applied
        await EnsureCandidateExistsAsync(newUser.Id);

        return _mapper.Map<UserProfileDto>(newUser);
    }

    private async Task EnsureCandidateExistsAsync(Guid userId)
    {
        // Check if candidate already exists
        var existingCandidate = await _candidateService.GetByUserIdAsync(userId);
        
        if (!existingCandidate.IsSuccess || existingCandidate.Value == null || existingCandidate.Value.Count == 0)
        {
            // Create candidate if doesn't exist
            var candidateDto = new CandidateDto
            {
                UserId = userId,
                CvFileId = null
            };
            await _candidateService.CreateAsync(candidateDto);
        }
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        var spec = new UserProfileByEmailSpec(email);
        var user = await _userProfileRepository.FirstOrDefaultAsync(spec);
        return user != null;
    }

    private async Task<UserProfileDto?> GetExistingUserAsync(string email)
    {
        var spec = new UserProfileByEmailSpec(email);
        var user = await _userProfileRepository.FirstOrDefaultAsync(spec);
        return user != null ? _mapper.Map<UserProfileDto>(user) : null;
    }

}
