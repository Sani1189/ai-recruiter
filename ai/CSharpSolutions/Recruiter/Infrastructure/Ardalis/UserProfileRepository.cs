using Recruiter.Application.UserProfile.Interfaces;
using Recruiter.Domain.Models;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Ardalis;

public class UserProfileRepository : EfRepository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(RecruiterDbContext context) : base(context)
    {
    }
}
