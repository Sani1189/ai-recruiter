using System;

namespace Recruiter.Application.ElevenLabs.Dto;

public class ConversationTokenResponseDto
{
    public string Token { get; set; } = string.Empty;

    public string? ConversationId { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? AgentId { get; set; }
    public string? SessionPayload { get; set; }
}

