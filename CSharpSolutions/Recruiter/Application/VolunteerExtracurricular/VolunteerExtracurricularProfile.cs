using AutoMapper;
using Recruiter.Application.VolunteerExtracurricular.Dto;

namespace Recruiter.Application.VolunteerExtracurricular;

public class VolunteerExtracurricularProfile : Profile
{
    public VolunteerExtracurricularProfile()
    {
        CreateMap<Domain.Models.VolunteerExtracurricular, VolunteerExtracurricularDto>();
        CreateMap<VolunteerExtracurricularDto, Domain.Models.VolunteerExtracurricular>();
    }
}
