using AutoMapper;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost;

public class JobPostProfile : Profile
{
    public JobPostProfile()
    {
        CreateMap<Domain.Models.JobPost, JobPostDto>()
            // Don't map StepAssignments here - orchestrator will manually populate AssignedSteps
            .ForMember(dest => dest.AssignedSteps, opt => opt.Ignore())
            .ForMember(dest => dest.OriginCountryCode, opt => opt.MapFrom(src => src.OriginCountryCode))
            .ForMember(dest => dest.CountryExposureCountryCodes, opt => opt.MapFrom(src =>
                src.CountryExposureSet != null && src.CountryExposureSet.Countries != null
                    ? src.CountryExposureSet.Countries.Select(c => c.CountryCode).ToList()
                    : new List<string>()));
        CreateMap<JobPostDto, Domain.Models.JobPost>()
            .ForMember(dest => dest.StepAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.CountryExposureSet, opt => opt.Ignore())
            .ForMember(dest => dest.OriginCountry, opt => opt.Ignore());
        
        CreateMap<JobPostStep, JobPostStepDto>();
        CreateMap<JobPostStepDto, JobPostStep>();
        
        CreateMap<JobPostStepAssignment, JobPostStepAssignmentDto>()
            .ForMember(dest => dest.StepName, opt => opt.MapFrom(src => src.StepName))
            .ForMember(dest => dest.StepVersion, opt => opt.MapFrom(src => src.StepVersion))
            // Don't map StepDetails here - service will manually populate it based on StepVersion (null or specific)
            .ForMember(dest => dest.StepDetails, opt => opt.Ignore());
        CreateMap<JobPostStepAssignmentDto, JobPostStepAssignment>()
            .ForMember(dest => dest.StepName, opt => opt.MapFrom(src => 
                !string.IsNullOrEmpty(src.StepName) ? src.StepName : 
                (src.StepDetails != null ? src.StepDetails.Name : string.Empty)))
            // CRITICAL: Use StepVersion as-is (null = use latest dynamically), never fall back to StepDetails.Version
            .ForMember(dest => dest.StepVersion, opt => opt.MapFrom(src => src.StepVersion))
            .ForMember(dest => dest.JobPost, opt => opt.Ignore())
            .ForMember(dest => dest.JobPostStep, opt => opt.Ignore());
            
        // Map JobPostStep to JobStepVersionDto (for version listing)
        CreateMap<JobPostStep, JobStepVersionDto>();   
    }
}
