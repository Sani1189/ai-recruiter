using Ardalis.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.Questionnaire.Dto;
using Recruiter.Application.Questionnaire.Interfaces;

namespace Recruiter.WebApi.Endpoints.Candidate;

[ApiController]
[Route("api/candidate/questionnaire")]
[Authorize(Policy = "Candidate")]
public sealed class QuestionnaireController(ICandidateQuestionnaireService service) : ControllerBase
{
    private readonly ICandidateQuestionnaireService _service = service;

    /// <summary>
    /// Returns the candidate-safe questionnaire template for a specific JobApplicationStep.
    /// This is used by the candidate interview page to render Questionnaire steps.
    /// </summary>
    [HttpGet("steps/{jobApplicationStepId:guid}/template")]
    public async Task<ActionResult<CandidateQuestionnaireTemplateDto>> GetTemplateForStep(
        Guid jobApplicationStepId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetTemplateForMyStepAsync(jobApplicationStepId, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Unauthorized => Forbid(),
                ResultStatus.Invalid => BadRequest(result.ValidationErrors),
                _ => BadRequest(result.ValidationErrors)
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Submits the candidate's answers for a Questionnaire step and marks the JobApplicationStep as completed.
    /// Candidates must submit before proceeding to next step.
    /// </summary>
    [HttpPost("steps/{jobApplicationStepId:guid}/submit")]
    public async Task<ActionResult<CandidateQuestionnaireSubmitResponseDto>> SubmitForStep(
        Guid jobApplicationStepId,
        [FromBody] CandidateQuestionnaireSubmitRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.SubmitForMyStepAsync(jobApplicationStepId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Unauthorized => Forbid(),
                ResultStatus.Invalid => BadRequest(result.ValidationErrors),
                _ => BadRequest(result.ValidationErrors)
            };
        }

        return Ok(result.Value);
    }
}

