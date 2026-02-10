using AutoMapper;
using Recruiter.Application.Experience.Dto;

namespace Recruiter.Application.Experience;

public class ExperienceProfile : Profile
{
    public ExperienceProfile()
    {
        CreateMap<Domain.Models.Experience, ExperienceDto>();
        CreateMap<ExperienceDto, Domain.Models.Experience>();
    }
}
