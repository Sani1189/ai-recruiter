using AutoMapper;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplication;

public class JobApplicationProfile : Profile
{
    public JobApplicationProfile()
    {
        CreateMap<Domain.Models.JobApplication, JobApplicationDto>()
            .ForMember(dest => dest.JobPost, opt => opt.MapFrom(src => src.JobPost));
        CreateMap<JobApplicationDto, Domain.Models.JobApplication>()
            .ForMember(dest => dest.JobPost, opt => opt.Ignore());
    }
}
