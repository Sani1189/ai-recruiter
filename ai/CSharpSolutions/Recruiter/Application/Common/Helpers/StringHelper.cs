using System.Text;
using System.Text.RegularExpressions;

namespace Recruiter.Application.Common.Helpers;

/// <summary>
/// High-performance string utilities for name generation and formatting
/// </summary>
public static class StringHelper
{
    private static readonly Regex NonAlphanumericRegex = new(@"[^a-z0-9]+", RegexOptions.Compiled);
    private static readonly Regex MultipleUnderscoresRegex = new(@"_{2,}", RegexOptions.Compiled);

    /// <summary>
    /// Converts text to a URL-friendly slug (lowercase, underscores, alphanumeric only)
    /// High-performance implementation with minimal allocations
    /// </summary>
    /// <param name="text">Input text to slugify</param>
    /// <param name="maxLength">Maximum length of the slug (default: 255)</param>
    /// <returns>Slugified string or empty if input is null/empty</returns>
    public static string Slugify(string? text, int maxLength = 255)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase and trim
        var slug = text.ToLowerInvariant().Trim();

        // Truncate if needed before processing
        if (slug.Length > maxLength)
            slug = slug[..maxLength];

        // Replace non-alphanumeric characters with underscores
        slug = NonAlphanumericRegex.Replace(slug, "_");

        // Replace multiple consecutive underscores with single underscore
        slug = MultipleUnderscoresRegex.Replace(slug, "_");

        // Remove leading/trailing underscores
        slug = slug.Trim('_');

        return slug;
    }

    /// <summary>
    /// Ensures name uniqueness by appending a numeric suffix if needed
    /// </summary>
    /// <param name="baseName">Base name to make unique</param>
    /// <param name="existingNames">Set of existing names to check against</param>
    /// <param name="maxLength">Maximum length of the final name</param>
    /// <returns>Unique name with suffix if necessary</returns>
    public static string EnsureUniqueName(string baseName, HashSet<string> existingNames, int maxLength = 255)
    {
        if (string.IsNullOrWhiteSpace(baseName))
            return string.Empty;

        if (!existingNames.Contains(baseName))
            return baseName;

        // Calculate max length for base name to leave room for suffix
        var suffixLength = 4; // "_999" worst case for reasonable templates
        var maxBaseLength = maxLength - suffixLength;
        var truncatedBase = baseName.Length > maxBaseLength 
            ? baseName[..maxBaseLength] 
            : baseName;

        // Find next available number
        for (var i = 2; i < 1000; i++)
        {
            var candidateName = $"{truncatedBase}_{i}";
            if (!existingNames.Contains(candidateName))
                return candidateName;
        }

        // Fallback: append timestamp
        return $"{truncatedBase}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }

    /// <summary>
    /// Validates that a name conforms to the expected format
    /// </summary>
    /// <param name="name">Name to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Only lowercase letters, numbers, and underscores
        return Regex.IsMatch(name, @"^[a-z0-9_]+$");
    }
}
