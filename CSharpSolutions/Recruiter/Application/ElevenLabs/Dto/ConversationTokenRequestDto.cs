
using System;

namespace Recruiter.Application.ElevenLabs.Dto;

public class ConversationTokenRequestDto
{
    public string? AgentId { get; set; }

    public string? InterviewConfigurationName { get; set; }

    public int? InterviewConfigurationVersion { get; set; }

    public string? JobPostName { get; set; }

    public int? JobPostVersion { get; set; }

    public Guid? JobApplicationId { get; set; }

    public string? StepName { get; set; }

    /// <summary>
    /// Optional UI-friendly label for the step. If provided, the ElevenLabs prompt builder should prefer this over StepName.
    /// </summary>
    public string? StepDisplayTitle { get; set; }
}
