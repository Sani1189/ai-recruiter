using AutoMapper;
using Recruiter.Application.Comment.Dto;
using Recruiter.Domain.Models;

namespace Recruiter.Application.Comment;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<Domain.Models.Comment, CommentDto>();
        CreateMap<CommentDto, Domain.Models.Comment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}

