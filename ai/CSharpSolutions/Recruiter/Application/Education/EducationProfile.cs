using AutoMapper;
using Recruiter.Application.Education.Dto;

namespace Recruiter.Application.Education;

public class EducationProfile : Profile
{
    public EducationProfile()
    {
        CreateMap<Domain.Models.Education, EducationDto>();
        CreateMap<EducationDto, Domain.Models.Education>();
    }
}
