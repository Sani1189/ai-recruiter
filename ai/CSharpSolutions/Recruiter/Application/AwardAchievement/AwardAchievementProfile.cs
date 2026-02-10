using AutoMapper;
using Recruiter.Application.AwardAchievement.Dto;

namespace Recruiter.Application.AwardAchievement;

public class AwardAchievementProfile : Profile
{
    public AwardAchievementProfile()
    {
        CreateMap<Domain.Models.AwardAchievement, AwardAchievementDto>();
        CreateMap<AwardAchievementDto, Domain.Models.AwardAchievement>();
    }
}
