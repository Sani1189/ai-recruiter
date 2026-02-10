namespace Recruiter.Application.Common.Options;

/// <summary>
/// Configuration options for Python API authentication and connection
/// </summary>
public class PythonApiOptions
{
    public const string SectionName = "PythonApi";

    public string BaseUrl { get; set; } = string.Empty;


    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;


    public string Scope { get; set; } = string.Empty;


    public string Authority => $"https://login.microsoftonline.com/{TenantId}";

    public int TimeoutSeconds { get; set; } = 30;

    public string? FunctionKey { get; set; }
}

