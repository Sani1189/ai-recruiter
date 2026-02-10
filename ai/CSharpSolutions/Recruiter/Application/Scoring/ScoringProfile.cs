using AutoMapper;
using Recruiter.Application.Scoring.Dto;

namespace Recruiter.Application.Scoring;

public class ScoringProfile : Profile
{
    public ScoringProfile()
    {
        CreateMap<Domain.Models.Scoring, ScoringDto>();
        CreateMap<ScoringDto, Domain.Models.Scoring>();
    }
}
