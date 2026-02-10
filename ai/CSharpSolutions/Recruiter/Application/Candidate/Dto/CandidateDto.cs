using System.ComponentModel.DataAnnotations;
using Recruiter.Application.Comment.Dto;
using Recruiter.Application.Common.Dto;
using Recruiter.Application.File.Dto;
using Recruiter.Application.KeyStrength.Dto;
using Recruiter.Application.Scoring.Dto;
using Recruiter.Application.Summary.Dto;
using Recruiter.Application.UserProfile.Dto;

namespace Recruiter.Application.Candidate.Dto;

public class CandidateDto : BaseModelDto
{
    public string? CandidateId { get; set; }
    public Guid? CvFileId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    public UserProfileDto? UserProfile { get; set; }
    public FileDto? CvFile { get; set; }
    public CommentDto? Comment { get; set; }
    public Guid? LatestCvEvaluationId { get; set; }
    public List<ScoringDto> Scorings { get; set; } = new();
    public List<KeyStrengthDto> KeyStrengths { get; set; } = new();
    public List<SummaryDto> Summaries { get; set; } = new();
}
