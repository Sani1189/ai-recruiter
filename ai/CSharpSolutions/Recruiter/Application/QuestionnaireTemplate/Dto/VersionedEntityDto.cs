namespace Recruiter.Application.QuestionnaireTemplate.Dto;

public abstract class VersionedEntityDto
{
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
}
