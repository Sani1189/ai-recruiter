using AutoMapper;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Domain.Models;
using Recruiter.Domain.Enums;

namespace Recruiter.Application.JobApplication;

public class JobApplicationStepProfile : Profile
{
    public JobApplicationStepProfile()
    {
        CreateMap<Domain.Models.JobApplicationStep, JobApplicationStepDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<JobApplicationStepDto, Domain.Models.JobApplicationStep>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => ParseStatusOrDefault(s.Status)));
    }

    private static JobApplicationStepStatusEnum ParseStatusOrDefault(string? value)
    {
        return Enum.TryParse<JobApplicationStepStatusEnum>(value, true, out var parsed)
            ? parsed
            : JobApplicationStepStatusEnum.Pending;
    }
}
