using AutoMapper;
using Recruiter.Application.CvEvaluation.Dto;

namespace Recruiter.Application.CvEvaluation;

public class CvEvaluationProfile : Profile
{
    public CvEvaluationProfile()
    {
        CreateMap<Domain.Models.CvEvaluation, CvEvaluationDto>();
        CreateMap<CvEvaluationDto, Domain.Models.CvEvaluation>()
            .ForMember(dest => dest.Scorings, opt => opt.Ignore())
            .ForMember(dest => dest.UserProfile, opt => opt.Ignore())
            .ForMember(dest => dest.File, opt => opt.Ignore());
    }
}
