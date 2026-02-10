using Ardalis.Specification;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class InterviewConfigurationsUsingPromptSpec : Specification<Recruiter.Domain.Models.InterviewConfiguration>
{
    public InterviewConfigurationsUsingPromptSpec(string promptName, int promptVersion)
    {
        Query.Where(ic =>
            (ic.InstructionPromptName == promptName && ic.InstructionPromptVersion == promptVersion) ||
            (ic.PersonalityPromptName == promptName && ic.PersonalityPromptVersion == promptVersion) ||
            (ic.QuestionsPromptName == promptName && ic.QuestionsPromptVersion == promptVersion));
    }
}



