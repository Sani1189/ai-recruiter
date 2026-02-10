using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobApplication.Specifications;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles.Services;

// Implementation of IJobApplicationStepResolver.
// Handles resolution of JobApplicationStep entities based on JobPostStep configuration.
public class JobApplicationStepResolver : IJobApplicationStepResolver
{
    private readonly IRepository<JobApplicationStep> _stepRepository;
    private readonly IRepository<JobPostStep> _jobPostStepRepository;

    public JobApplicationStepResolver(
        IRepository<JobApplicationStep> stepRepository,
        IRepository<JobPostStep> jobPostStepRepository)
    {
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _jobPostStepRepository = jobPostStepRepository ?? throw new ArgumentNullException(nameof(jobPostStepRepository));
    }

    public async Task<JobApplicationStep> GetOrCreateStepAsync(
        Guid jobApplicationId,
        string stepName,
        int? stepVersion,
        CancellationToken cancellationToken = default)
    {
        // Resolve step version if null (use latest)
        var resolvedVersion = stepVersion ?? await ResolveLatestStepVersionAsync(stepName, cancellationToken);

        var existingSteps = await _stepRepository.ListAsync(
            new JobApplicationStepByJobApplicationIdSpec(jobApplicationId),
            cancellationToken);

        var existingStep = existingSteps.FirstOrDefault(s => 
            s.JobPostStepName == stepName && 
            s.JobPostStepVersion == resolvedVersion);

        if (existingStep != null)
        {
            // Update status to completed if not already
            if (existingStep.Status != JobApplicationStepStatusEnum.Completed)
            {
                existingStep.Status = JobApplicationStepStatusEnum.Completed;
                existingStep.CompletedAt = DateTimeOffset.UtcNow;
                existingStep.StartedAt ??= existingStep.CompletedAt;
                await _stepRepository.UpdateAsync(existingStep);
                await _stepRepository.SaveChangesAsync(cancellationToken);
            }
            return existingStep;
        }

        // Calculate step number based on existing steps
        var stepNumber = existingSteps.Any() ? existingSteps.Max(s => s.StepNumber) + 1 : 1;

        var newStep = new JobApplicationStep
        {
            JobApplicationId = jobApplicationId,
            JobPostStepName = stepName,
            JobPostStepVersion = resolvedVersion,
            StepNumber = stepNumber,
            Status = JobApplicationStepStatusEnum.Completed,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };

        await _stepRepository.AddAsync(newStep);
        await _stepRepository.SaveChangesAsync(cancellationToken);

        return newStep;
    }

    public async Task<JobPostStep?> GetJobPostStepAsync(
        string stepName,
        int? stepVersion,
        CancellationToken cancellationToken = default)
    {
        // If stepVersion is null, get latest version
        if (stepVersion == null)
        {
            var latestSpec = new JobPostStepLatestByNameSpec(stepName);
            return await _jobPostStepRepository.FirstOrDefaultAsync(latestSpec, cancellationToken);
        }

        var spec = new JobPostStepByNameAndVersionSpec(stepName, stepVersion.Value);
        return await _jobPostStepRepository.FirstOrDefaultAsync(spec, cancellationToken);
    }

    private async Task<int> ResolveLatestStepVersionAsync(string stepName, CancellationToken cancellationToken)
    {
        var latestSpec = new JobPostStepLatestByNameSpec(stepName);
        var latestStep = await _jobPostStepRepository.FirstOrDefaultAsync(latestSpec, cancellationToken);
        return latestStep?.Version ?? 1; // Default to version 1 if no step found
    }
}

