using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.ElevenLabs.Dto;
using Recruiter.Application.ElevenLabs.Interfaces;
using Recruiter.Application.InterviewConfiguration.Dto;
using Recruiter.Application.InterviewConfiguration.Interfaces;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;

namespace Recruiter.Application.ElevenLabs;

public class ElevenLabsConversationPayloadBuilder : IElevenLabsConversationPayloadBuilder
{
    private const string DefaultCompanyName = "Osilion AS";
    private static readonly Regex QuestionPattern = new(@"^(\d+\.|[-*•])\s?", RegexOptions.Compiled);

    private readonly IInterviewConfigurationService _interviewConfigurationService;
    private readonly IJobPostService _jobPostService;
    private readonly ICurrentUserService _currentUserService;

    public ElevenLabsConversationPayloadBuilder(
        IInterviewConfigurationService interviewConfigurationService,
        IJobPostService jobPostService,
        ICurrentUserService currentUserService)
    {
        _interviewConfigurationService = interviewConfigurationService ?? throw new ArgumentNullException(nameof(interviewConfigurationService));
        _jobPostService = jobPostService ?? throw new ArgumentNullException(nameof(jobPostService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<Result<Dictionary<string, object?>>> BuildPayloadAsync(
        ConversationTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.InterviewConfigurationName))
        {
            return Result<Dictionary<string, object?>>.Invalid(new ValidationError
            {
                ErrorMessage = "Interview configuration name is required."
            });
        }

        var configuration = await LoadInterviewConfigurationAsync(
            request.InterviewConfigurationName,
            request.InterviewConfigurationVersion);

        if (configuration == null)
        {
            return Result<Dictionary<string, object?>>.NotFound(
                $"Interview configuration '{request.InterviewConfigurationName}' (version {(request.InterviewConfigurationVersion?.ToString() ?? "latest")}) was not found.");
        }

        var jobPost = await LoadJobPostAsync(request.JobPostName, request.JobPostVersion);
        if (!string.IsNullOrWhiteSpace(request.JobPostName)
            && request.JobPostVersion.HasValue
            && jobPost == null)
        {
            return Result<Dictionary<string, object?>>.NotFound(
                $"Job post '{request.JobPostName}' v{request.JobPostVersion} was not found.");
        }

        // Build the configuration payload to be used by frontend during startSession
        var candidateName = _currentUserService.GetCurrentUserInfo()?.Name;
        var stepLabel = !string.IsNullOrWhiteSpace(request.StepDisplayTitle)
            ? request.StepDisplayTitle
            : request.StepName;
        var dynamicVariables = BuildDynamicVariables(configuration, jobPost, candidateName, stepLabel);

        // CRITICAL: Agent has {{first_message}} placeholder - only pass dynamic_variables for substitution
        // DO NOT use agent_config_override for first_message to avoid conflicts
        var payload = new Dictionary<string, object?>
        {
            // Pass dynamic variables at top level - ElevenLabs will substitute placeholders
            ["dynamic_variables"] = dynamicVariables,
            
            // Also pass in nested format for compatibility
            ["conversation_initiation_client_data"] = new Dictionary<string, object?>
            {
                ["dynamic_variables"] = dynamicVariables
            }
        };

        return Result<Dictionary<string, object?>>.Success(payload);
    }

    private async Task<InterviewConfigurationWithPromptsDto?> LoadInterviewConfigurationAsync(string name, int? version)
    {
        if (version.HasValue)
        {
            return await _interviewConfigurationService.GetWithResolvedPromptsAsync(name, version.Value);
        }

        return await _interviewConfigurationService.GetLatestWithResolvedPromptsAsync(name);
    }

    private async Task<JobPostDto?> LoadJobPostAsync(string? name, int? version)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (version.HasValue && version.Value > 0)
        {
            return await _jobPostService.GetByIdAsync(name, version.Value);
        }

        return await _jobPostService.GetLatestVersionAsync(name);
    }

    private static Dictionary<string, string> BuildDynamicVariables(
        InterviewConfigurationWithPromptsDto configuration,
        JobPostDto? jobPost,
        string? candidateName = null,
        string? stepName = null)
    {
        var jobTitle = ResolveJobTitle(jobPost);
        var instruction = ReplaceTemplateVariables(configuration.InstructionPrompt?.Content ?? string.Empty, jobPost);
        var personality = ReplaceTemplateVariables(configuration.PersonalityPrompt?.Content ?? string.Empty, jobPost);
        var questionsPrompt = ReplaceTemplateVariables(configuration.QuestionsPrompt?.Content ?? string.Empty, jobPost);
        var questions = BuildQuestions(instruction, questionsPrompt);
        var duration = configuration.Duration ?? 30;

        var variables = new Dictionary<string, string>
        {
            ["introduction"] = BuildIntroduction(jobTitle),
            ["tone"] = configuration.Tone ?? "Professional",
            ["focus_area"] = configuration.FocusArea ?? "General",
            ["duration"] = duration.ToString(),
            ["job_title"] = jobTitle,
            ["company_name"] = DefaultCompanyName,
            ["instruction_prompt"] = instruction,
            ["personality_prompt"] = personality,
            ["questions_prompt"] = questionsPrompt,
            ["questions"] = string.Join("\n", questions),
            ["first_message"] = BuildFirstMessage(jobTitle, duration, candidateName, stepName),
            ["rules"] = BuildRules(),
            ["behavioral_rules"] = BuildBehavioralRules()
        };

        // Build comprehensive combined prompt for frontend
        variables["combined_prompt"] = BuildComprehensivePrompt(variables, duration);

        return variables;
    }

    private static string BuildIntroduction(string jobTitle) =>
        $"You are interviewer conducting a structured interview for the {jobTitle} role at {DefaultCompanyName}. " +
        $"Your purpose is to assess the candidate fairly and professionally. Maintain a neutral, professional tone and focus on " +
        $"understanding the candidate's skills and thinking. Do not coach or correct. Keep the interview on track and time-conscious.";

    private static string BuildRules() =>
        "Assess, don't coach. Ask concise, relevant follow-ups. Use clear, professional language. " +
        "Keep context from prior answers. If the candidate is confused, briefly clarify intent without giving hints. " +
        "Move on politely when answers are insufficient. follow the instructions and questions prompts given to you carefully. Do not ask in-depth questions unless instructed to do so.";

    private static string BuildBehavioralRules() =>
        "**NEVER REMAIN SILENT:** If you are ever unsure how to respond, you MUST ask for clarification. Use phrases like: \"Could you please elaborate on that?\" or \"I want to make sure I understood, can you rephrase that?\"\n" +
        "**HUMAN-LIKE PACING:** Wait for the candidate to finish speaking before you respond. A pause of 3-5 seconds is natural. Do not rush.\n" +
        "**AVOID JARGON EXPLAINING ITSELF:** Don't explain AI or your own nature. Just act as a human recruiter would."+
        "**NEVER REVEAL THE INTERVIEWER'S IDENTITY:** Do not reveal that you are an AI interviewer. You are a human recruiter. Also do not disclose any information about company policies or procedures.";

    private static string BuildFirstMessage(string jobTitle, int duration, string? candidateName = null, string? stepName = null)
    {
        var greeting = !string.IsNullOrWhiteSpace(candidateName)
            ? $"Hi {candidateName}, "
            : "Hi, ";

        var stepSegment = !string.IsNullOrWhiteSpace(stepName)
            ? $"\"{stepName}\" segment"
            : "interview segment";

        return $"{greeting}welcome to your {jobTitle} interview with {DefaultCompanyName}. We're about to begin the {stepSegment}. Please let me know when you're ready to start.";
    }

    private static string BuildComprehensivePrompt(Dictionary<string, string> variables, int duration)
    {
        return string.Join("\n", new[]
        {
            "[BEHAVIORAL RULES]",
            GetVariable(variables, "behavioral_rules"),
            string.Empty,
            "[DURATION CONTROL]",
            $"The total interview duration is set to {duration} minutes. When you have 1-2 minutes remaining, naturally begin wrapping up the conversation and thanking the candidate.",
            "Your host application will send you a system command to wrap up near the end. When you receive the `[WRAP_UP_NOW]` command, begin closing politely but KEEP LISTENING and responding until the candidate confirms they are done OR until you receive `[THANKS_FOR_YOUR_TIME]`. Do not go silent after a single reply. When you receive `[THANKS_FOR_YOUR_TIME]`, immediately thank the candidate and end the interview.",
            string.Empty,
            "[INTERVIEW DETAILS]",
            $"Tone: {GetVariable(variables, "tone", "Professional")}",
            $"Focus Area: {GetVariable(variables, "focus_area", "General")}",
            string.Empty,
            "Interview Instructions:",
            GetVariable(variables, "instruction_prompt"),
            string.Empty,
            "Interviewer Personality:",
            GetVariable(variables, "personality_prompt"),
            string.Empty,
            "Question Guidance:",
            GetVariable(variables, "questions_prompt"),
            string.Empty,
            "Optional Questions (reference as needed, ask naturally):",
            GetVariable(variables, "questions"),
            string.Empty,
            "Rules:",
            GetVariable(variables, "rules")
        });
    }

    private static IEnumerable<string> BuildQuestions(params string[] sources)
    {
        foreach (var source in sources)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                continue;
            }

            var lines = source.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.EndsWith("?", StringComparison.Ordinal) || QuestionPattern.IsMatch(line))
                {
                    yield return line;
                }
            }
        }
    }

    private static string ReplaceTemplateVariables(string? text, JobPostDto? jobPost)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var jobTitle = ResolveJobTitle(jobPost);
        var jobDescription = jobPost?.JobDescription ?? string.Empty;
        var experienceLevel = jobPost?.ExperienceLevel ?? string.Empty;
        var employmentType = jobPost?.JobType ?? string.Empty;

        return text
            .Replace("{{jobTitle}}", jobTitle)
            .Replace("{{companyName}}", DefaultCompanyName)
            .Replace("{{jobDescription}}", jobDescription)
            .Replace("{{experienceLevel}}", experienceLevel)
            .Replace("{{employmentType}}", employmentType);
    }

    private static string ResolveJobTitle(JobPostDto? jobPost)
    {
        return string.IsNullOrWhiteSpace(jobPost?.JobTitle)
            ? "this position"
            : jobPost.JobTitle;
    }

    private static string GetVariable(
        IReadOnlyDictionary<string, string> variables,
        string key,
        string fallback = "")
    {
        if (variables.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }
}

// {{introduction}}

// Tone: {{tone}}
// Focus Area: {{focus_area}}
// Duration: {{duration}} minutes

// Interview Instructions:
// {{instruction_prompt}}


// Interviewer Personality:
// {{personality_prompt}}

// Question Guidance:
// {{questions_prompt}}

// Optional Questions (reference as needed, ask naturally):
// {{questions}}

// Rules:{{rules}}