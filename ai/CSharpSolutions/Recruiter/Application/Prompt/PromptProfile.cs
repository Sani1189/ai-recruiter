using AutoMapper;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Prompt;

public class PromptProfile : Profile
{
    public PromptProfile()
    {
        CreateMap<Domain.Models.Prompt, PromptDto>();
        CreateMap<PromptDto, Domain.Models.Prompt>();
    }
}
