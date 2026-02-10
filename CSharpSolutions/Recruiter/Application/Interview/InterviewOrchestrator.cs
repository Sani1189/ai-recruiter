using Ardalis.Result;
using FluentValidation;
using Recruiter.Application.Interview.Dto;
using Recruiter.Application.Interview.Interfaces;

namespace Recruiter.Application.Interview;

// Interview Orchestrator for complex business operations
public interface IInterviewOrchestrator
{
    Task<Result<InterviewDto>> GetInterviewWithScoresAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<InterviewDto>> ScheduleInterviewAsync(InterviewDto interviewDto, CancellationToken cancellationToken = default);
    Task<Result<InterviewDto>> CompleteInterviewAsync(Guid id, CancellationToken cancellationToken = default);
}

// Interview Orchestrator implementation
public class InterviewOrchestrator : IInterviewOrchestrator
{
    private readonly IInterviewService _interviewService;
    private readonly IValidator<InterviewDto> _interviewValidator;

    public InterviewOrchestrator(
        IInterviewService interviewService,
        IValidator<InterviewDto> interviewValidator)
    {
        _interviewService = interviewService;
        _interviewValidator = interviewValidator;
    }

    public async Task<Result<InterviewDto>> GetInterviewWithScoresAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var interview = await _interviewService.GetByIdAsync(id);
        if (interview == null)
        {
            return Result<InterviewDto>.NotFound($"Interview with ID {id} not found");
        }

        // Get related scores and feedback would be handled here
        // This is a simple implementation - in real scenario you might want to include scores in the DTO

        return Result<InterviewDto>.Success(interview);
    }

    public async Task<Result<InterviewDto>> ScheduleInterviewAsync(InterviewDto interviewDto, CancellationToken cancellationToken = default)
    {
        // Validate the interview
        var validationResult = await _interviewValidator.ValidateAsync(interviewDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<InterviewDto>.Invalid(validationResult.Errors.Select(e => new ValidationError { ErrorMessage = e.ErrorMessage }).ToArray());
        }

        // Process the interview (create or update)
        InterviewDto result;
        if (interviewDto.Id == Guid.Empty)
        {
            result = await _interviewService.CreateAsync(interviewDto);
        }
        else
        {
            result = await _interviewService.UpdateAsync(interviewDto);
        }

        return Result<InterviewDto>.Success(result);
    }

    public async Task<Result<InterviewDto>> CompleteInterviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var interview = await _interviewService.GetByIdAsync(id);
        if (interview == null)
        {
            return Result<InterviewDto>.NotFound($"Interview with ID {id} not found");
        }

        // Update completed date
        interview.CompletedAt = DateTimeOffset.UtcNow;
        var updatedInterview = await _interviewService.UpdateAsync(interview);

        return Result<InterviewDto>.Success(updatedInterview);
    }
}
