using Recruiter.Application.Common.Dto;

namespace Recruiter.Application.JobApplicationStepFiles.Dto;

public class JobApplicationStepFilesDto : BaseModelDto
{
    public Guid FileId { get; set; }
    public Guid JobApplicationStepId { get; set; }
}
