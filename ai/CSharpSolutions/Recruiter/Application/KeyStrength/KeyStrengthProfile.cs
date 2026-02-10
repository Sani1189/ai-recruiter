using AutoMapper;
using Recruiter.Application.KeyStrength.Dto;

namespace Recruiter.Application.KeyStrength;

public class KeyStrengthProfile : Profile
{
    public KeyStrengthProfile()
    {
        CreateMap<Domain.Models.KeyStrength, KeyStrengthDto>();
        CreateMap<KeyStrengthDto, Domain.Models.KeyStrength>();
    }
}
