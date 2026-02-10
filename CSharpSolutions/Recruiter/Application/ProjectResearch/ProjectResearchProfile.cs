using AutoMapper;
using Recruiter.Application.ProjectResearch.Dto;

namespace Recruiter.Application.ProjectResearch;

public class ProjectResearchProfile : Profile
{
    public ProjectResearchProfile()
    {
        CreateMap<Domain.Models.ProjectResearch, ProjectResearchDto>();
        CreateMap<ProjectResearchDto, Domain.Models.ProjectResearch>();
    }
}
