namespace Recruiter.Application.JobPost.Exceptions;

/// <summary>
/// Thrown when a job post exists but is not in Published status (e.g. Draft or Archived).
/// Used by public/candidate endpoints to return 404 with errorCode JOB_NOT_AVAILABLE.
/// </summary>
public class JobPostNotAvailableException : InvalidOperationException
{
    public const string ErrorCode = "JOB_NOT_AVAILABLE";

    public JobPostNotAvailableException()
        : base("This job is not available at the moment.")
    {
    }

    public JobPostNotAvailableException(string message)
        : base(message)
    {
    }
}
