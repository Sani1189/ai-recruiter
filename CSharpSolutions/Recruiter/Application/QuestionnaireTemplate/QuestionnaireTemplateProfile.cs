using AutoMapper;
using Recruiter.Application.QuestionnaireTemplate.Dto;

namespace Recruiter.Application.QuestionnaireTemplate;

public class QuestionnaireTemplateProfile : Profile
{
    public QuestionnaireTemplateProfile()
    {
        CreateMap<Domain.Models.QuestionnaireTemplate, QuestionnaireTemplateDto>()
            .ForMember(d => d.TemplateType, opt => opt.MapFrom(s => s.TemplateType.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
            .ForMember(d => d.IsPublished,
                opt => opt.MapFrom(s => string.Equals(s.Status, "Published", StringComparison.OrdinalIgnoreCase)))
            .ForMember(d => d.SectionsCount, opt => opt.MapFrom(s => s.Sections.Count))
            .ForMember(d => d.QuestionsCount,
                opt => opt.MapFrom(s => s.Sections.SelectMany(sec => sec.Questions.Where(q => q.IsActive)).Count()));
        CreateMap<Domain.Models.QuestionnaireSection, QuestionnaireSectionDto>();
        CreateMap<Domain.Models.QuestionnaireQuestion, QuestionnaireQuestionDto>()
            .ForMember(d => d.QuestionType, opt => opt.MapFrom(s => s.QuestionType.ToString()))
            .ForMember(d => d.PromptText, opt => opt.MapFrom(s => s.QuestionText));
        CreateMap<Domain.Models.QuestionnaireQuestionOption, QuestionnaireOptionDto>();

        CreateMap<Domain.Models.QuestionnaireQuestion, QuestionnaireQuestionHistoryDetailsDto>()
            .ForMember(d => d.QuestionType, opt => opt.MapFrom(s => s.QuestionType.ToString()))
            .ForMember(d => d.PromptText, opt => opt.MapFrom(s => s.QuestionText));

        CreateMap<Domain.Models.QuestionnaireTemplate, CandidateQuestionnaireTemplateDto>()
            .ForMember(d => d.TemplateType, opt => opt.MapFrom(s => s.TemplateType.ToString()));
        CreateMap<Domain.Models.QuestionnaireSection, CandidateQuestionnaireSectionDto>();
        CreateMap<Domain.Models.QuestionnaireQuestion, CandidateQuestionnaireQuestionDto>()
            .ForMember(d => d.QuestionType, opt => opt.MapFrom(s => s.QuestionType.ToString()))
            .ForMember(d => d.PromptText, opt => opt.MapFrom(s => s.QuestionText))
            .ForMember(d => d.Options, opt => opt.MapFrom(s => s.Options));
        CreateMap<Domain.Models.QuestionnaireQuestionOption, CandidateQuestionnaireOptionDto>();
    }
}


