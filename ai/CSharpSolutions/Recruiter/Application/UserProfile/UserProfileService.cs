using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.UserProfile.Dto;
using Recruiter.Application.UserProfile.Interfaces;
using Ardalis.Result;

namespace Recruiter.Application.UserProfile;

public class UserProfileService : IUserProfileService
{
    private readonly IRepository<Domain.Models.UserProfile> _repository;
    private readonly IMapper _mapper;
    private readonly Queries.UserProfileQueryHandler _queryHandler;

    public UserProfileService(IRepository<Domain.Models.UserProfile> repository, IMapper mapper, Queries.UserProfileQueryHandler queryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
    }

    public async Task<IEnumerable<UserProfileDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<UserProfileDto>>(entities);
    }

    public async Task<UserProfileDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<UserProfileDto>(entity) : null;
    }

    public async Task<UserProfileDto> CreateAsync(UserProfileDto dto)
    {
        // Normalize email to lowercase for consistency
        var normalizedEmail = !string.IsNullOrEmpty(dto.Email) ? dto.Email.ToLowerInvariant() : string.Empty;
        
        // Check if a user profile with the same email already exists
        if (!string.IsNullOrEmpty(normalizedEmail))
        {
            var existingUserResult = await GetByEmailAsync(normalizedEmail);
            if (existingUserResult.IsSuccess && existingUserResult.Value != null)
            {
                throw new InvalidOperationException($"A UserProfile with email '{normalizedEmail}' already exists.");
            }
        }

        // Update DTO with normalized email before mapping
        dto.Email = normalizedEmail;
        var entity = _mapper.Map<Domain.Models.UserProfile>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<UserProfileDto>(entity);
    }

    public async Task<UserProfileDto> UpdateAsync(UserProfileDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"UserProfile with id '{dto.Id}' not found.");
        }

        // Update the existing entity properties directly - EF Core will detect changes automatically
        // Exclude Email field as it's tied to Microsoft/Google account and should not be updated
        // Exclude ResumeUrl field as it's managed by the system through CV upload process
        _mapper.Map(dto, entity, opt => opt.AfterMap((src, dest) => 
        {
            // Preserve the original email - don't update it
            dest.Email = entity.Email;
            // Preserve the original resumeUrl - don't update it (managed by system)
            dest.ResumeUrl = entity.ResumeUrl;
        }));
        // CreatedAt, CreatedBy, UpdatedAt, UpdatedBy are handled by AuditInterceptor

        // No need for UpdateAsync - EF Core detects changes on tracked entities
        await _repository.SaveChangesAsync();

        return _mapper.Map<UserProfileDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity != null)
        {
            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null;
    }

    // Delegate complex queries to QueryHandler
    public async Task<Result<UserProfileDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByEmailAsync(email, cancellationToken);
    }

    public async Task<Result<List<UserProfileDto>>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByNameAsync(name, cancellationToken);
    }

    public async Task<Result<List<UserProfileDto>>> GetByNationalityAsync(string nationality, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByNationalityAsync(nationality, cancellationToken);
    }

    public async Task<Result<List<UserProfileDto>>> GetByAgeRangeAsync(int minAge, int maxAge, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetByAgeRangeAsync(minAge, maxAge, cancellationToken);
    }

    public async Task<Result<Common.Dto.PagedResult<UserProfileDto>>> GetFilteredUserProfilesAsync(UserProfileListQueryDto query, CancellationToken cancellationToken = default)
    {
        return await _queryHandler.GetFilteredUserProfilesAsync(query, cancellationToken);
    }

    public async Task<Result<UserProfileDetailsDto>> GetUserProfileDetailsAsync(Guid userId)
    {
        return await _queryHandler.GetUserProfileDetailsAsync(userId);
    }

    /// <summary>
    /// Updates the current user profile without causing Entity Framework tracking conflicts
    /// This method handles the tracking issue by updating the existing entity directly
    /// </summary>
    public async Task<UserProfileDto> UpdateCurrentUserProfileAsync(UserProfileDto userProfileDto)
    {
        // Get the existing entity by ID
        var existingEntity = await _repository.GetByIdAsync(userProfileDto.Id!.Value);
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"User profile with id '{userProfileDto.Id}' not found.");
        }

        // Update the existing entity properties directly - EF Core will detect changes automatically
        // Exclude Email field as it's tied to Microsoft/Google account and should not be updated
        // Exclude ResumeUrl field as it's managed by the system through CV upload process
        _mapper.Map(userProfileDto, existingEntity, opt => opt.AfterMap((src, dest) => 
        {
            // Preserve the original email - don't update it
            dest.Email = existingEntity.Email;
            // Preserve the original resumeUrl - don't update it (managed by system)
            dest.ResumeUrl = existingEntity.ResumeUrl;
        }));
        // CreatedAt, CreatedBy, UpdatedAt, UpdatedBy are handled by AuditInterceptor

        // No need for UpdateAsync - EF Core detects changes on tracked entities
        await _repository.SaveChangesAsync();

        return _mapper.Map<UserProfileDto>(existingEntity);
    }
}
