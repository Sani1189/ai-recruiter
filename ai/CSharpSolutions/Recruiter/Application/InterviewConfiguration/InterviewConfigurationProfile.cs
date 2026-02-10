using AutoMapper;
using Recruiter.Application.InterviewConfiguration.Dto;
using Recruiter.Application.Prompt.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.InterviewConfiguration;

public class InterviewConfigurationProfile : Profile
{
    public InterviewConfigurationProfile()
    {
        CreateMap<Domain.Models.InterviewConfiguration, InterviewConfigurationDto>();
        CreateMap<InterviewConfigurationDto, Domain.Models.InterviewConfiguration>();
        
        CreateMap<Domain.Models.InterviewConfiguration, InterviewConfigurationWithPromptsDto>();
        CreateMap<Domain.Models.Prompt, PromptDto>();
        CreateMap<Domain.Models.Prompt, PromptVersionDto>();
    }
}
