namespace Recruiter.Application.JobApplicationStepFiles.Strategies;

// Default strategy for generic file uploads that don't require special processing.
// Can be extended in the future for prompt-based processing or other requirements.
public class GenericFileUploadProcessingStrategy : IFileUploadProcessingStrategy
{
    public bool CanHandle(string stepType) => true; // Default handler for all step types

    public Task ProcessFileUploadAsync(
        FileUploadProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        // Future: Could implement prompt-based processing here
        // For now, generic uploads don't require additional processing
        if (!string.IsNullOrEmpty(context.PromptName))
        {
            Console.WriteLine($"Generic file upload with prompt {context.PromptName} v{context.PromptVersion} - processing not yet implemented");
        }

        return Task.CompletedTask;
    }
}

