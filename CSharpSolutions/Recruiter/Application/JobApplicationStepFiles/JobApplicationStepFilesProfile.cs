using AutoMapper;
using Recruiter.Application.JobApplicationStepFiles.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobApplicationStepFiles;

public class JobApplicationStepFilesProfile : Profile
{
    public JobApplicationStepFilesProfile()
    {
        CreateMap<Domain.Models.JobApplicationStepFiles, JobApplicationStepFilesDto>();
        CreateMap<JobApplicationStepFilesDto, Domain.Models.JobApplicationStepFiles>();
    }
}
