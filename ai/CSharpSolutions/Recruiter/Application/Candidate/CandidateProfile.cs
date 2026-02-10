using AutoMapper;
using Recruiter.Application.Candidate.Dto;
using Recruiter.Application.UserProfile.Dto;
using Recruiter.Application.File.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Candidate;

public class CandidateProfile : Profile
{
    public CandidateProfile()
    {
        // Enhanced Candidate mapping with UserProfile and CvFile
        CreateMap<Domain.Models.Candidate, CandidateDto>()
            .ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src.UserProfile))
            .ForMember(dest => dest.CvFile, opt => opt.MapFrom(src => src.CvFile));
        
        // Reverse mapping for updates (exclude UserProfile and CvFile from updates)s
        CreateMap<CandidateDto, Domain.Models.Candidate>()
            .ForMember(dest => dest.UserProfile, opt => opt.Ignore())
            .ForMember(dest => dest.CvFile, opt => opt.Ignore());
    }
}
