using System;
using System.Collections.Generic;

namespace Recruiter.Application.ElevenLabs.Dto;

public class ConversationDetailsDto
{
    public string ConversationId { get; set; } = string.Empty;

    public Uri ConversationUrl { get; set; } = new("https://api.elevenlabs.io/");

    public Uri AudioUrl { get; set; } = new("https://api.elevenlabs.io/");

    public string? Status { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public List<TranscriptEntryDto>? Transcript { get; set; }
}

public class TranscriptEntryDto
{
    public string? Role { get; set; }
    public string? Source { get; set; }
    public string? Speaker { get; set; }
    public string? Text { get; set; }
    public string? Content { get; set; }
    public string? Message { get; set; }
}

