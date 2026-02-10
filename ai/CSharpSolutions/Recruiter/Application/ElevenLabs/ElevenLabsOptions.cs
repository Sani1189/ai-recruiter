using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.ElevenLabs;

public class ElevenLabsOptions
{
    public const string SectionName = "ElevenLabs";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://api.elevenlabs.io";

    public string ApiKey { get; set; } = string.Empty;

    public string AgentId { get; set; } = string.Empty;

    [Required]
    public string TokenEndpoint { get; set; } = "/v1/convai/conversation/token";

    [Required]
    public string ConversationEndpoint { get; set; } = "/v1/convai/conversations";

    [Range(1, 300)]
    public int MaxRequestsPerMinute { get; set; } = 30;

    [Required]
    public string AudioContentType { get; set; } = "audio/mpeg";

    /// <summary>
    /// Secret key used to validate webhook signatures from ElevenLabs
    /// </summary>
    [Required]
    public string WebhookSecret { get; set; } = string.Empty;
}

