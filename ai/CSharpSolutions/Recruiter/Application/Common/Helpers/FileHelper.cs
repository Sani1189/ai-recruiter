using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Recruiter.Application.Common.Helpers;

public static class FileHelper
{
    private static readonly Dictionary<string, string> KnownMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".txt"] = "text/plain"
    };

    public static (string SafeFileName, string Extension) SanitizeFileName(string fileName)
    {
        var trimmedName = string.IsNullOrWhiteSpace(fileName) ? "resume" : Path.GetFileName(fileName);
        var extension = Path.GetExtension(trimmedName);
        
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".dat";
        
        extension = extension.ToLowerInvariant();

        var baseName = Path.GetFileNameWithoutExtension(trimmedName);
        baseName = Regex.Replace(string.IsNullOrWhiteSpace(baseName) ? "resume" : baseName, @"[^a-zA-Z0-9\-]+", "-").Trim('-');
        
        if (string.IsNullOrEmpty(baseName))
            baseName = "resume";

        return ($"{baseName.ToLowerInvariant()}{extension}", extension);
    }

    public static string ResolveContentType(string? providedContentType, string extension)
    {
        if (!string.IsNullOrWhiteSpace(providedContentType))
            return providedContentType;

        return KnownMimeTypes.TryGetValue(extension, out var mime) 
            ? mime 
            : "application/octet-stream";
    }

    public static int CalculateFileSizeInMb(long bytes)
    {
        if (bytes <= 0)
            return 0;

        return (int)Math.Max(1, Math.Ceiling(bytes / (1024.0 * 1024.0)));
    }

    public static (string FolderName, string FileName) ExtractPathParts(string blobPath, string fallbackFileName)
    {
        var pathParts = blobPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var folderName = pathParts.Length > 1 
            ? string.Join("/", pathParts.Take(pathParts.Length - 1)) 
            : string.Empty;
        var fileName = pathParts.LastOrDefault() ?? fallbackFileName;
        return (folderName, fileName);
    }
}

