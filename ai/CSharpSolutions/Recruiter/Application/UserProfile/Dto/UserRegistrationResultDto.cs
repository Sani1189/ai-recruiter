using Recruiter.Application.Candidate.Dto;

namespace Recruiter.Application.UserProfile.Dto;

public class UserRegistrationResultDto
{
    public UserProfileDto UserProfile { get; set; } = null!;
    public CandidateDto? Candidate { get; set; }
    public bool UserCreated { get; set; }
    public bool CandidateCreated { get; set; }
    public string Message { get; set; } = string.Empty;
}
