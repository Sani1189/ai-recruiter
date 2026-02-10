using Ardalis.Specification;

namespace Recruiter.Application.Prompt.Specifications;

public sealed class InterviewsUsingPromptSpec : Specification<Recruiter.Domain.Models.Interview>
{
    public InterviewsUsingPromptSpec(string promptName, int promptVersion)
    {
        Query.Where(i =>
            (i.InstructionPromptName == promptName && i.InstructionPromptVersion == promptVersion) ||
            (i.PersonalityPromptName == promptName && i.PersonalityPromptVersion == promptVersion) ||
            (i.QuestionsPromptName == promptName && i.QuestionsPromptVersion == promptVersion));
    }
}



