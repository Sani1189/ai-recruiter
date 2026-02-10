using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles.Services;

// Service for resolving and managing JobApplicationStep entities.
// Handles creation, retrieval, and updates of steps based on JobPostStep configuration.
public interface IJobApplicationStepResolver
{
    // Gets or creates a JobApplicationStep for the given job application and step name
    // If stepVersion is null, uses the latest version of the step
    Task<JobApplicationStep> GetOrCreateStepAsync(
        Guid jobApplicationId,
        string stepName,
        int? stepVersion,
        CancellationToken cancellationToken = default);

    // Gets the JobPostStep configuration for a given step name and version
    // If stepVersion is null, returns the latest version of the step
    Task<JobPostStep?> GetJobPostStepAsync(
        string stepName,
        int? stepVersion,
        CancellationToken cancellationToken = default);
}


