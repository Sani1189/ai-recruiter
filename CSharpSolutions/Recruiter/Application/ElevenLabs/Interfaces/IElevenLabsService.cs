using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Recruiter.Application.ElevenLabs.Dto;

namespace Recruiter.Application.ElevenLabs.Interfaces;

public interface IElevenLabsService
{
    Task<Result<ConversationTokenResponseDto>> CreateConversationTokenAsync(
        ConversationTokenRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<ConversationDetailsDto>> GetConversationAsync(
        string conversationId,
        CancellationToken cancellationToken = default);

    Task<Result<ConversationAudioContent>> GetConversationAudioAsync(
        string conversationId,
        CancellationToken cancellationToken = default);
}

