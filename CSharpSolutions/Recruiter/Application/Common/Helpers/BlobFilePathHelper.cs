namespace Recruiter.Application.Common.Helpers;

public static class BlobFilePathHelper
{
    /// <summary>
    /// Build folder path for job application files
    /// Example: ("candidates/job-applications/backend-dev/v1/guid123", "resume_abc.pdf")
    /// Structure: candidates/job-applications/{jobName}/v{version}/{jobApplicationId}/{uniqueGuid}.{ext}
    /// </summary>
    public static (string FolderName, string FileName) BuildJobApplicationPath(string jobName, int jobPostVersion, Guid appId, string extension = "pdf")
    {
        var cleanJobName = jobName.Trim().ToLowerInvariant().Replace(" ", "-");
        var folderName = $"candidates/job-applications/{cleanJobName}/v{jobPostVersion}/{appId:N}";
        var ext = extension.StartsWith(".") ? extension : $".{extension}";
        var fileName = $"{Guid.NewGuid():N}{ext}";
        return (folderName, fileName);
    }

    /// <summary>
    /// Build folder path for candidate CV evaluation
    /// </summary>
    public static (string FolderName, string FileName) BuildCvEvaluationPath(Guid candidateId)
    {
        var folderName = $"candidates/cv-evaluations/{candidateId:N}";
        var fileName = $"{Guid.NewGuid():N}.pdf";
        return (folderName, fileName);
    }

    /// <summary>
    /// Build folder path for candidate documents
    /// </summary>
    public static (string FolderName, string FileName) BuildCandidateDocumentPath(Guid candidateId, string extension)
    {
        var folderName = $"candidates/documents/{candidateId:N}";
        var fileName = $"{Guid.NewGuid():N}{extension}";
        return (folderName, fileName);
    }

    /// <summary>
    /// Generate unique filename with extension
    /// </summary>
    public static string GenerateFileName(string extension)
    {
        if (!extension.StartsWith("."))
            extension = "." + extension;
        return $"{Guid.NewGuid():N}{extension}";
    }

    /// <summary>
    /// Get file extension without dot
    /// </summary>
    public static string GetExtension(string fileName)
    {
        return Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
    }

    /// <summary>
    /// Get full blob path from folder and filename
    /// </summary>
    public static string GetFullBlobPath(string folderName, string fileName)
    {
        return string.IsNullOrEmpty(folderName) ? fileName : $"{folderName}/{fileName}";
    }
}

