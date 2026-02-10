using AutoMapper;
using Recruiter.Application.UserProfile.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.UserProfile;

public class UserProfileProfile : Profile
{
    public UserProfileProfile()
    {
        // Convert NULL to false when mapping Entity to DTO
        CreateMap<Domain.Models.UserProfile, UserProfileDto>()
            .ForMember(dest => dest.OpenToRelocation, opt => opt.MapFrom(src => src.OpenToRelocation ?? false));
        
        // Convert NULL to false when mapping DTO to Entity  
        CreateMap<UserProfileDto, Domain.Models.UserProfile>()
            .ForMember(dest => dest.OpenToRelocation, opt => opt.MapFrom(src => src.OpenToRelocation ?? false));

        // Map UserProfile to UserProfileDetailsDto with all related entities
        CreateMap<Domain.Models.UserProfile, UserProfileDetailsDto>()
            .ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Educations, opt => opt.MapFrom(src => src.Educations))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills))
            .ForMember(dest => dest.Summaries, opt => opt.MapFrom(src => src.Summaries))
            .ForMember(dest => dest.AwardAchievements, opt => opt.MapFrom(src => src.AwardAchievements))
            .ForMember(dest => dest.CertificationLicenses, opt => opt.MapFrom(src => src.CertificationLicenses))
            .ForMember(dest => dest.Experiences, opt => opt.MapFrom(src => src.Experiences))
            .ForMember(dest => dest.ProjectResearch, opt => opt.MapFrom(src => src.ProjectResearches))
            .ForMember(dest => dest.KeyStrengths, opt => opt.MapFrom(src => src.KeyStrengths))
            .ForMember(dest => dest.VolunteerExtracurriculars, opt => opt.MapFrom(src => src.VolunteerExtracurriculars));
    }
}
