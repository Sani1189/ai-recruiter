using AutoMapper;
using Recruiter.Application.Skill.Dto;

namespace Recruiter.Application.Skill;

public class SkillProfile : Profile
{
    public SkillProfile()
    {
        CreateMap<Domain.Models.Skill, SkillDto>();
        CreateMap<SkillDto, Domain.Models.Skill>();
    }
}
