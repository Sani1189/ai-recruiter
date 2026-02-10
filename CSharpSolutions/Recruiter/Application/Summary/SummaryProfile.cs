using AutoMapper;
using Recruiter.Application.Summary.Dto;

namespace Recruiter.Application.Summary;

public class SummaryProfile : Profile
{
    public SummaryProfile()
    {
        CreateMap<Domain.Models.Summary, SummaryDto>();
        CreateMap<SummaryDto, Domain.Models.Summary>();
    }
}
