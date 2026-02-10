namespace Recruiter.Domain.Enums;

/// <summary>Job post lifecycle: Draft, Published, or Archived (when deleted and has applications).</summary>
public enum JobPostStatusEnum
{
    Draft,
    Published,
    Archived
}
