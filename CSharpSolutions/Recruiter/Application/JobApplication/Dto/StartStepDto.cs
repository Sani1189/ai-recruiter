using System.ComponentModel.DataAnnotations;

namespace Recruiter.Application.JobApplication.Dto;

public class StartStepDto
{
    [Required]
    public Guid StepId { get; set; }
}


