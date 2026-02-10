using System;

namespace Recruiter.Application.ElevenLabs.Dto;
public class ConversationAudioContent
{
    public byte[] Data { get; init; } = Array.Empty<byte>();

    public string ContentType { get; init; } = "audio/mpeg";

    public string? FileName { get; init; }

    public long? ContentLength => Data?.LongLength ?? 0;
}

