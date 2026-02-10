using Ardalis.Specification;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class InterviewConfigurationsUsingPromptNameWithDynamicLatestSpec : Specification<Recruiter.Domain.Models.InterviewConfiguration>
{
    public InterviewConfigurationsUsingPromptNameWithDynamicLatestSpec(string promptName)
    {
        // PromptVersion == null means "use latest version dynamically"
        Query.Where(ic =>
            (ic.InstructionPromptName == promptName && ic.InstructionPromptVersion == null) ||
            (ic.PersonalityPromptName == promptName && ic.PersonalityPromptVersion == null) ||
            (ic.QuestionsPromptName == promptName && ic.QuestionsPromptVersion == null));
    }
}


