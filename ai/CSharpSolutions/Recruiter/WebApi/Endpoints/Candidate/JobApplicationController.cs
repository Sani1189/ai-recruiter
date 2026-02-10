using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Recruiter.Application.JobApplication.Interfaces;
using Recruiter.Application.JobApplication.Dto;
using Recruiter.Application.JobApplicationStepFiles.Dto;
using Recruiter.Application.JobApplicationStepFiles.Interfaces;
using Recruiter.Application.Common.Dto;
using Microsoft.AspNetCore.Http;
using Ardalis.Result;

namespace Recruiter.WebApi.Endpoints.Candidate;

[ApiController]
[Route("api/candidate/job-application")]
[Authorize(Policy = "Candidate")]
public class JobApplicationController(
    IJobApplicationStepFilesService jobApplicationStepFilesService,
    IJobApplicationStepService jobApplicationStepService,
    IJobApplicationService jobApplicationService,
    IJobApplicationCandidateFlowService candidateFlowService) : ControllerBase
{
    private readonly IJobApplicationStepFilesService _jobApplicationStepFilesService = jobApplicationStepFilesService;
    private readonly IJobApplicationStepService _jobApplicationStepService = jobApplicationStepService;
    private readonly IJobApplicationService _jobApplicationService = jobApplicationService;
    private readonly IJobApplicationCandidateFlowService _candidateFlowService = candidateFlowService;

    #region Job Application Management

    // Get my job applications (for candidates)
    [HttpGet("my-applications")]
    public async Task<ActionResult<List<JobApplicationDto>>> GetMyApplications()
    {
        var result = await _jobApplicationService.GetMyApplicationsAsync();
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    // Apply for a job (for candidates)
    [HttpPost]
    public async Task<ActionResult<JobApplicationDto>> ApplyForJob([FromBody] ApplyForJobDto applyForJobDto)
    {
        if (applyForJobDto == null)
            return BadRequest("Job application data is required");

        var result = await _jobApplicationService.ApplyForJobAsync(applyForJobDto);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);

        return CreatedAtAction(nameof(GetMyApplication), new { id = result.Value.Id }, result.Value);
    }

    // Get my application details (for candidates)
    [HttpGet("my-application/{id}")]
    public async Task<ActionResult<JobApplicationDto>> GetMyApplication(Guid id)
    {
        var result = await _jobApplicationService.GetMyApplicationAsync(id);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Unauthorized => Forbid("You can only view your own applications"),
                _ => BadRequest(result.ValidationErrors)
            };
        }
        return Ok(result.Value);
    }

    // Get my job application for a specific job post (targeted lookup)
    [HttpGet("my-application/jobpost/{jobPostName}/{jobPostVersion:int}")]
    public async Task<ActionResult<JobApplicationDto>> GetMyApplicationByJobPost(string jobPostName, int jobPostVersion, CancellationToken cancellationToken)
    {
        var result = await _candidateFlowService.GetMyApplicationByJobPostAsync(jobPostName, jobPostVersion, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.Unauthorized => Forbid(),
                _ => BadRequest(result.ValidationErrors)
            };
        }

        if (result.Value == null)
        {
            return NotFound();
        }

        return Ok(result.Value);
    }

    // Get my application progress (application + steps) for a job post (does NOT create anything)
    [HttpGet("my-application/jobpost/{jobPostName}/{jobPostVersion:int}/progress")]
    public async Task<ActionResult<MyJobApplicationProgressDto>> GetMyApplicationProgress(string jobPostName, int jobPostVersion, CancellationToken cancellationToken)
    {
        var result = await _candidateFlowService.GetMyProgressAsync(jobPostName, jobPostVersion, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.Unauthorized => Forbid(),
                _ => BadRequest(result.ValidationErrors)
            };
        }

        return Ok(result.Value);
    }

    // Begin a step for a job post (idempotent): creates/gets application + step and marks it started; ensures interview if needed
    [HttpPost("jobpost/{jobPostName}/{jobPostVersion:int}/steps/begin")]
    public async Task<ActionResult<BeginJobApplicationStepResponseDto>> BeginStep(
        string jobPostName,
        int jobPostVersion,
        [FromBody] BeginJobApplicationStepRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _candidateFlowService.BeginStepAsync(jobPostName, jobPostVersion, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.Invalid => BadRequest(result.ValidationErrors),
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Unauthorized => Forbid(),
                _ => BadRequest(result.ValidationErrors)
            };
        }

        return Ok(result.Value);
    }

    // Filtered my applications list (paged)
    [HttpGet("my-applications/filtered")]
    public async Task<ActionResult<Recruiter.Application.Common.Dto.PagedResult<JobApplicationDto>>> GetMyApplicationsFiltered([FromQuery] JobApplicationListQueryDto query)
    {
        var result = await _jobApplicationService.GetMyApplicationsPagedAsync(query);
        if (!result.IsSuccess)
            return BadRequest(result.ValidationErrors);
        return Ok(result.Value);
    }

    #endregion

    #region File Upload for Steps

    // Generic endpoints for any step type

    [HttpPost("steps/upload/get-upload-url")]
    public async Task<ActionResult<Recruiter.Application.JobApplicationStepFiles.Dto.GetUploadUrlResponseDto>> GetStepUploadUrl(
        [FromBody] Recruiter.Application.JobApplicationStepFiles.Dto.GetUploadUrlRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _jobApplicationStepFilesService.GetUploadUrlAsync(request);
        
        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors?.FirstOrDefault() 
                ?? result.ValidationErrors?.FirstOrDefault()?.ErrorMessage 
                ?? "Failed to generate upload URL";
            return BadRequest(new { message = errorMessage, errors = result.ValidationErrors });
        }
        
        return Ok(result.Value);
    }

    [HttpPost("steps/upload/complete")]
    public async Task<ActionResult<UploadResumeResultDto>> CompleteStepUpload([FromBody] CompleteUploadRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _jobApplicationStepFilesService.CompleteUploadAsync(request);
        
        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors?.FirstOrDefault() 
                ?? result.ValidationErrors?.FirstOrDefault()?.ErrorMessage 
                ?? "Failed to complete upload";
            return BadRequest(new { message = errorMessage, errors = result.ValidationErrors });
        }
        
        return Ok(result.Value);
    }

    [HttpPost("steps/upload")]
    [RequestSizeLimit(50_000_000)] // 50MB limit
    public async Task<ActionResult<UploadResumeResultDto>> UploadStepFile(
        [FromForm] IFormFile file, 
        [FromForm] string jobPostName, 
        [FromForm] int jobPostVersion,
        [FromForm] string stepName,
        [FromForm] int? stepVersion) // Nullable - null means use latest version
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        if (string.IsNullOrWhiteSpace(jobPostName))
            return BadRequest(new { message = "Job post name is required" });

        if (string.IsNullOrWhiteSpace(stepName))
            return BadRequest(new { message = "Step name is required" });

        if (stepVersion.HasValue && stepVersion < 1)
            return BadRequest(new { message = "Step version must be greater than 0" });

        const long maxFileSize = 5_000_000; // 5MB
        if (file.Length > maxFileSize)
            return BadRequest(new { message = $"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB" });

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var result = await _jobApplicationStepFilesService.UploadStepFileAsync(
            fileData, file.FileName, file.ContentType, jobPostName, jobPostVersion, stepName, stepVersion);
        
        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors?.FirstOrDefault() 
                ?? result.ValidationErrors?.FirstOrDefault()?.ErrorMessage 
                ?? "Upload failed";
            return BadRequest(new { message = errorMessage, errors = result.ValidationErrors });
        }
        
        return Ok(result.Value);
    }

    #endregion

    #region Job Application Step Management

    // Get steps for my job application (for candidates)
    [HttpGet("steps/{applicationId}")]
    public async Task<ActionResult<List<JobApplicationStepDto>>> GetMyApplicationSteps(Guid applicationId)
    {
        var result = await _jobApplicationStepService.GetMyApplicationStepsAsync(applicationId);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.Unauthorized => Forbid("You can only view steps for your own applications"),
                _ => BadRequest(result.ValidationErrors)
            };
        }
        return Ok(result.Value);
    }

    // Update my step (submit data, files, etc.) (for candidates)
    [HttpPut("steps/{stepId}")]
    public async Task<ActionResult<JobApplicationStepDto>> UpdateMyStep(Guid stepId, [FromBody] JobApplicationStepDto stepDto)
    {
        if (stepDto == null)
            return BadRequest("Step data is required");

        var result = await _jobApplicationStepService.UpdateMyStepAsync(stepId, stepDto);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Unauthorized => Forbid("You can only update your own application steps"),
                _ => BadRequest(result.ValidationErrors)
            };
        }
        return Ok(result.Value);
    }

    // Complete my step (for candidates)
    [HttpPut("steps/{stepId}/complete")]
    public async Task<ActionResult<JobApplicationStepDto>> CompleteMyStep(Guid stepId, [FromBody] CompleteStepDto completeDto)
    {
        var result = await _jobApplicationStepService.CompleteMyStepAsync(stepId, completeDto?.Data);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Unauthorized => Forbid("You can only complete your own application steps"),
                _ => BadRequest(result.ValidationErrors)
            };
        }
        return Ok(result.Value);
    }

    // Start my step (for candidates)
    [HttpPut("steps/{stepId}/start")]
    public async Task<ActionResult<JobApplicationStepDto>> StartMyStep(Guid stepId)
    {
        var result = await _jobApplicationStepService.StartMyStepAsync(stepId);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Unauthorized => Forbid("You can only start your own application steps"),
                _ => BadRequest(result.ValidationErrors)
            };
        }
        return Ok(result.Value);
    }

    // Create a step for my application (for candidates)
    [HttpPost("{applicationId}/steps")]
    public async Task<ActionResult<JobApplicationStepDto>> CreateMyStep(Guid applicationId, [FromBody] Recruiter.Application.JobApplication.Dto.CreateStepDto createDto)
    {
        if (createDto == null)
            return BadRequest("Step data is required");

        var result = await _jobApplicationStepService.CreateMyStepAsync(applicationId, createDto);
        if (!result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Unauthorized => Forbid("You can only create steps for your own applications"),
                _ => BadRequest(result.ValidationErrors)
            };
        }
        return Ok(result.Value);
    }

    // Submit my application (explicit completion)
    [HttpPut("{applicationId}/submit")]
    public async Task<ActionResult<JobApplicationDto>> SubmitMyApplication(Guid applicationId)
    {
        var appResult = await _jobApplicationStepService.GetMyApplicationStepsAsync(applicationId);
        if (appResult.Status == ResultStatus.Unauthorized)
            return Forbid();

        // Mark application CompletedAt regardless of remaining steps; client decides when to call
        var jobAppService = HttpContext.RequestServices.GetRequiredService<IJobApplicationService>();
        var updated = await jobAppService.UpdateApplicationStatusAsync(applicationId, "Completed");
        if (!updated.IsSuccess)
            return BadRequest(updated.ValidationErrors);
        return Ok(updated.Value);
    }

    #endregion
}
