using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile.Interfaces;

public interface IUserProfileRepository : IRepository<Domain.Models.UserProfile>
{
}
