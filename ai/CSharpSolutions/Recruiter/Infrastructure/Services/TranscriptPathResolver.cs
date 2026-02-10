using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.Application.Interview.Interfaces;
using Recruiter.Infrastructure.Repository;

namespace Recruiter.Infrastructure.Services;

/// <summary>
/// Resolves transcript storage paths by querying Interview → JobApplicationStep → JobApplication chain
/// </summary>
public class TranscriptPathResolver : ITranscriptPathResolver
{
    private readonly RecruiterDbContext _dbContext;
    private readonly ILogger<TranscriptPathResolver> _logger;
    
    private const string TranscriptFolder = "candidates/job-applications";
    private const string TranscriptFileName = "transcript.json";

    public TranscriptPathResolver(
        RecruiterDbContext dbContext,
        ILogger<TranscriptPathResolver> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TranscriptPathResult?> ResolvePathAsync(
        string conversationId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            _logger.LogWarning("Cannot resolve transcript path: conversation_id is null or empty");
            return null;
        }

        // TranscriptUrl pattern: <base-url>/api/candidate/ai-interview/conversations/conv_<conversation-id>
        // We search for records where TranscriptUrl ends with the conversation_id
        var searchPattern = $"conv_{conversationId}";

        try
        {
            // Query Interview → JobApplicationStep → JobApplication
            var interview = await _dbContext.Interviews
                .Include(i => i.JobApplicationStep)
                    .ThenInclude(jas => jas!.JobApplication)
                .Where(i => i.TranscriptUrl != null && i.TranscriptUrl.EndsWith(searchPattern))
                .FirstOrDefaultAsync(cancellationToken);

            if (interview is null)
            {
                _logger.LogWarning(
                    "No interview found for conversation_id: {ConversationId} (searched for TranscriptUrl ending with '{Pattern}')",
                    conversationId, searchPattern);
                return null;
            }

            var jobApplicationStep = interview.JobApplicationStep;
            if (jobApplicationStep is null)
            {
                _logger.LogWarning(
                    "Interview {InterviewId} has no associated JobApplicationStep",
                    interview.Id);
                return null;
            }

            var jobApplication = jobApplicationStep.JobApplication;
            if (jobApplication is null)
            {
                _logger.LogWarning(
                    "JobApplicationStep {StepId} has no associated JobApplication",
                    jobApplicationStep.Id);
                return null;
            }

            // Build path: candidates/job-applications/{JobPostName}/v{JobPostVersion}/{JobApplicationId}/transcript.json
            var blobPath = $"{TranscriptFolder}/{jobApplication.JobPostName}/v{jobApplication.JobPostVersion}/{jobApplication.Id}/{TranscriptFileName}";

            _logger.LogInformation(
                "Resolved transcript path for conversation {ConversationId}: {BlobPath}",
                conversationId, blobPath);

            return new TranscriptPathResult(
                BlobPath: blobPath,
                JobApplicationId: jobApplication.Id,
                JobPostName: jobApplication.JobPostName,
                JobPostVersion: jobApplication.JobPostVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error resolving transcript path for conversation_id: {ConversationId}", 
                conversationId);
            return null;
        }
    }
}
