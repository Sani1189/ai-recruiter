using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.JobPost.Dto;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Application.JobApplication.Specifications;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost;

/// Service for handling job post candidate operations
public class JobPostCandidateService : IJobPostCandidateService
{
    private readonly IRepository<Domain.Models.JobApplication> _jobApplicationRepository;
    private readonly IRepository<Domain.Models.Candidate> _candidateRepository;
    private readonly IRepository<Domain.Models.UserProfile> _userProfileRepository;
    private readonly IRepository<Domain.Models.JobApplicationStep> _jobApplicationStepRepository;

    public JobPostCandidateService(
        IRepository<Domain.Models.JobApplication> jobApplicationRepository,
        IRepository<Domain.Models.Candidate> candidateRepository,
        IRepository<Domain.Models.UserProfile> userProfileRepository,
        IRepository<Domain.Models.JobApplicationStep> jobApplicationStepRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _candidateRepository = candidateRepository;
        _userProfileRepository = userProfileRepository;
        _jobApplicationStepRepository = jobApplicationStepRepository;
    }

    public async Task<Dictionary<string, int>> GetCandidateCountsAsync(IEnumerable<(string Name, int Version)> jobPosts, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, int>();
        
        // Count at database level for each job post instead of loading all applications
        foreach (var (name, version) in jobPosts)
        {
            var key = $"{name}_{version}";
            var spec = new JobApplicationByJobPostSpec(name, version);
            var count = await _jobApplicationRepository.CountAsync(spec, cancellationToken);
            result[key] = count;
        }

        return result;
    }

    public async Task<IEnumerable<JobPostCandidateDto>> GetJobPostCandidatesAsync(string jobPostName, int jobPostVersion, CancellationToken cancellationToken = default)
    {
        // Use specification to filter at database level and eager load related data
        var spec = new JobApplicationByJobPostSpec(jobPostName, jobPostVersion);
        var jobApplications = await _jobApplicationRepository.ListAsync(spec, cancellationToken);

        var result = new List<JobPostCandidateDto>();

        foreach (var application in jobApplications)
        {
            var candidate = application.Candidate;
            var userProfile = candidate?.UserProfile;
            var cvFile = candidate?.CvFile;

            // Get all application steps for this candidate to calculate current step
            var applicationStepsSpec = new JobApplicationStepByJobApplicationIdSpec(application.Id);
            var applicationSteps = await _jobApplicationStepRepository.ListAsync(applicationStepsSpec, cancellationToken);
            
            // Count completed steps
            var completedStepsCount = applicationSteps.Count(s => s.Status.ToString().ToLower() == "completed");
            
            // Current step is completed steps + 1
            var currentStep = completedStepsCount + 1;

            result.Add(new JobPostCandidateDto
            {
                ApplicationId = application.Id,
                CandidateId = application.CandidateId,
                CandidateSerial = candidate?.CandidateId ?? "Unknown",
                CandidateName = userProfile?.Name ?? "Unknown",
                CandidateEmail = userProfile?.Email ?? "Unknown",
                CandidateCvFilePath = cvFile?.FilePath,
                AppliedAt = application.CreatedAt != default(DateTimeOffset) ? application.CreatedAt.DateTime : DateTime.UtcNow,
                CompletedAt = application.CompletedAt?.DateTime,
                Status = "Applied",
                CurrentStep = currentStep,
                CompletedStepsCount = completedStepsCount
            });
        }

        return result;
    }

    public async Task<IEnumerable<JobPostCandidateDto>> GetAllCandidatesAsync(CancellationToken cancellationToken = default)
    {
        // Use specification with eager loading to avoid N+1 queries
        var spec = new AllJobApplicationsWithIncludesSpec();
        var applications = await _jobApplicationRepository.ListAsync(spec, cancellationToken);

        var result = new List<JobPostCandidateDto>();

        foreach (var application in applications)
        {
            var candidate = application.Candidate;
            var userProfile = candidate?.UserProfile;

            // Get all application steps for this candidate to calculate current step
            var applicationStepsSpec = new JobApplicationStepByJobApplicationIdSpec(application.Id);
            var applicationSteps = await _jobApplicationStepRepository.ListAsync(applicationStepsSpec, cancellationToken);
            
            // Count completed steps
            var completedStepsCount = applicationSteps.Count(s => s.Status.ToString().ToLower() == "completed");
            
            // Current step is completed steps + 1
            var currentStep = completedStepsCount + 1;

            result.Add(new JobPostCandidateDto
            {
                ApplicationId = application.Id,
                CandidateId = application.CandidateId,
                CandidateSerial = candidate?.CandidateId ?? "Unknown",
                CandidateName = userProfile?.Name ?? "Unknown",
                CandidateEmail = userProfile?.Email ?? "Unknown",
                AppliedAt = application.CreatedAt != default(DateTimeOffset) ? application.CreatedAt.DateTime : DateTime.UtcNow,
                CompletedAt = application.CompletedAt?.DateTime,
                Status = "Applied",
                CurrentStep = currentStep,
                CompletedStepsCount = completedStepsCount
            });
        }

        return result;
    }
}
