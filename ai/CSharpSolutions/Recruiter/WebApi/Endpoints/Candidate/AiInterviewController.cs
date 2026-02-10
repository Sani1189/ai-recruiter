using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Recruiter.Application.ElevenLabs.Dto;
using Recruiter.Application.ElevenLabs.Interfaces;
using Ardalis.Result;

namespace Recruiter.WebApi.Endpoints.Candidate;

[ApiController]
[Route("api/candidate/ai-interview")]
public class AiInterviewController : ControllerBase
{
    private readonly IElevenLabsService _elevenLabsService;
    private readonly ILogger<AiInterviewController> _logger;

    public AiInterviewController(
        IElevenLabsService elevenLabsService,
        ILogger<AiInterviewController> logger)
    {
        _elevenLabsService = elevenLabsService ?? throw new ArgumentNullException(nameof(elevenLabsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("token")]
    [Authorize(Policy = "Candidate")]
    public async Task<ActionResult<ConversationTokenResponseDto>> CreateConversationToken(
        [FromBody] ConversationTokenRequestDto? request,
        CancellationToken cancellationToken)
    {
        var safeRequest = request ?? new ConversationTokenRequestDto();
        var result = await _elevenLabsService.CreateConversationTokenAsync(safeRequest, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BuildErrorResult(result);
    }

    [HttpGet("conversations/{conversationId}")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<ActionResult<ConversationDetailsDto>> GetConversation(
        [FromRoute] string conversationId,
        CancellationToken cancellationToken)
    {
        var result = await _elevenLabsService.GetConversationAsync(conversationId, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BuildErrorResult(result);
    }

    [HttpGet("conversations/{conversationId}/audio")]
    [Authorize(Policy = "AdminOrCandidate")]
    public async Task<IActionResult> GetConversationAudio(
        [FromRoute] string conversationId,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _elevenLabsService.GetConversationAudioAsync(conversationId, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return BuildErrorResult(result);
        }

        var audio = result.Value;
        var fileName = !string.IsNullOrWhiteSpace(audio.FileName)
            ? audio.FileName
            : $"{conversationId}.mp3";

        Response.Headers.CacheControl = "private, max-age=300";
        if (download)
        {
            return File(audio.Data, audio.ContentType, fileName);
        }

        Response.Headers["Content-Disposition"] = "inline";
        return File(audio.Data, audio.ContentType);
    }

    private ActionResult BuildErrorResult<T>(Result<T> result)
    {
        _logger.LogWarning("AI interview proxy request failed with status {Status} and errors {Errors}",
            result.Status,
            string.Join("; ", result.Errors ?? Array.Empty<string>()));

        return result.Status switch
        {
            ResultStatus.Invalid => BadRequest(result.ValidationErrors),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Unauthorized => Unauthorized(),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Errors),
            _ => StatusCode(StatusCodes.Status502BadGateway,
                result.Errors?.Any() == true ? result.Errors : new[] { "Upstream ElevenLabs request failed." })
        };
    }
}

