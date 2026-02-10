using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Recruiter.Application.ElevenLabs.Dto;

namespace Recruiter.Application.ElevenLabs.Interfaces;

public interface IElevenLabsConversationPayloadBuilder
{
    Task<Result<Dictionary<string, object?>>> BuildPayloadAsync(
        ConversationTokenRequestDto request,
        CancellationToken cancellationToken = default);
}

