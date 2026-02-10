using AutoMapper;
using Recruiter.Application.Interview.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Interview;

public class InterviewProfile : Profile
{
    public InterviewProfile()
    {
        CreateMap<Domain.Models.Interview, InterviewDto>();
        CreateMap<InterviewDto, Domain.Models.Interview>();
    }
}
