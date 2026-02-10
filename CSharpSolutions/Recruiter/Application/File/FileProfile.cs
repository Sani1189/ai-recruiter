using AutoMapper;
using Recruiter.Application.File.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.File;

public class FileProfile : Profile
{
    public FileProfile()
    {
        CreateMap<Domain.Models.File, FileDto>();
        CreateMap<FileDto, Domain.Models.File>();
    }
}
