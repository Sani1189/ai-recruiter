using Ardalis.Result;
using AutoMapper;
using Recruiter.Application.UserProfile.Dto;
using Recruiter.Application.UserProfile.Specifications;
using Recruiter.Application.Common.Interfaces;

namespace Recruiter.Application.UserProfile.Queries;

// UserProfile query handler using Ardalis specification pattern
public class UserProfileQueryHandler
{
    private readonly IRepository<Domain.Models.UserProfile> _repository;
    private readonly IMapper _mapper;

    public UserProfileQueryHandler(IRepository<Domain.Models.UserProfile> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Get user profile by ID using specification pattern
    public async Task<Result<UserProfileDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result<UserProfileDto>.Invalid(new ValidationError { ErrorMessage = "Invalid user profile ID" });

        var spec = new UserProfileByIdSpec(id);
        var userProfile = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (userProfile == null)
            return Result<UserProfileDto>.NotFound($"User profile with ID {id} not found");

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(userProfileDto);
    }

    // Get user profile by email using specification pattern
    public async Task<Result<UserProfileDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<UserProfileDto>.Invalid(new ValidationError { ErrorMessage = "Invalid email" });

        var spec = new UserProfileByEmailSpec(email);
        var userProfile = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        
        if (userProfile == null)
            return Result<UserProfileDto>.NotFound($"User profile with email {email} not found");

        var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(userProfileDto);
    }

    // Get user profiles by name using specification pattern
    public async Task<Result<List<UserProfileDto>>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<List<UserProfileDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid name" });

        var spec = new UserProfilesByNameSpec(name);
        var userProfiles = await _repository.ListAsync(spec, cancellationToken);
        var userProfileDtos = _mapper.Map<List<UserProfileDto>>(userProfiles);
        
        return Result<List<UserProfileDto>>.Success(userProfileDtos);
    }

    // Get user profiles by nationality using specification pattern
    public async Task<Result<List<UserProfileDto>>> GetByNationalityAsync(string nationality, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nationality))
            return Result<List<UserProfileDto>>.Invalid(new ValidationError { ErrorMessage = "Invalid nationality" });

        var spec = new UserProfilesByNationalitySpec(nationality);
        var userProfiles = await _repository.ListAsync(spec, cancellationToken);
        var userProfileDtos = _mapper.Map<List<UserProfileDto>>(userProfiles);
        
        return Result<List<UserProfileDto>>.Success(userProfileDtos);
    }

    // Get user profiles by age range using specification pattern
    public async Task<Result<List<UserProfileDto>>> GetByAgeRangeAsync(int minAge, int maxAge, CancellationToken cancellationToken = default)
    {
        var spec = new UserProfilesByAgeRangeSpec(minAge, maxAge);
        var userProfiles = await _repository.ListAsync(spec, cancellationToken);
        var userProfileDtos = _mapper.Map<List<UserProfileDto>>(userProfiles);
        
        return Result<List<UserProfileDto>>.Success(userProfileDtos);
    }

    // Advanced query with filtering, sorting, and pagination using Ardalis specifications
    public async Task<Result<Common.Dto.PagedResult<UserProfileDto>>> GetFilteredUserProfilesAsync(UserProfileListQueryDto query, CancellationToken cancellationToken = default)
    {
        // Validate query parameters
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            // Use specifications for complex queries
            var countSpec = new UserProfileFilterCountSpec(query);
            var filterSpec = new UserProfileFilterSpec(query);

            // Get total count efficiently
            var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

            // Get filtered and paged results
            var userProfiles = await _repository.ListAsync(filterSpec, cancellationToken);

            // Map to DTOs
            var userProfileDtos = _mapper.Map<List<UserProfileDto>>(userProfiles);

            var result = new Common.Dto.PagedResult<UserProfileDto>
            {
                Items = userProfileDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Common.Dto.PagedResult<UserProfileDto>>.Success(result);
        }
        catch (Exception ex)
        {
            // Log exception (not shown here)
            Console.WriteLine(ex.Message);
            return Result<Common.Dto.PagedResult<UserProfileDto>>.Error();
        }
    }

    public async Task<Result<UserProfileDetailsDto>> GetUserProfileDetailsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return Result<UserProfileDetailsDto>.Invalid(new ValidationError { ErrorMessage = "Invalid user ID" });

        var spec = new Specifications.UserProfileDetailsByIdSpec(userId);
        var userProfile = await _repository.FirstOrDefaultAsync(spec);
        
        if (userProfile == null)
            return Result<UserProfileDetailsDto>.NotFound($"User profile with ID {userId} not found");

        var userProfileDetailsDto = _mapper.Map<UserProfileDetailsDto>(userProfile);
        return Result<UserProfileDetailsDto>.Success(userProfileDetailsDto);
    }
}
