using Ardalis.Result;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Queries;

public sealed class QuestionnaireTemplateQueryHandler
{
    private readonly IRepository<Domain.Models.QuestionnaireTemplate> _templateRepository;
    private readonly IRepository<QuestionnaireQuestion> _questionRepository;
    private readonly IRepository<JobPostStep> _jobPostStepRepository;
    private readonly IRepository<QuestionnaireCandidateSubmission> _submissionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<QuestionnaireTemplateQueryHandler> _logger;

    public QuestionnaireTemplateQueryHandler(
        IRepository<Domain.Models.QuestionnaireTemplate> templateRepository,
        IRepository<QuestionnaireQuestion> questionRepository,
        IRepository<JobPostStep> jobPostStepRepository,
        IRepository<QuestionnaireCandidateSubmission> submissionRepository,
        IMapper mapper,
        ILogger<QuestionnaireTemplateQueryHandler> logger)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        _jobPostStepRepository = jobPostStepRepository ?? throw new ArgumentNullException(nameof(jobPostStepRepository));
        _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<QuestionnaireTemplateDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _templateRepository.ListAsync(cancellationToken);
            var active = entities.Where(x => !x.IsDeleted).ToList();
            return Result<List<QuestionnaireTemplateDto>>.Success(_mapper.Map<List<QuestionnaireTemplateDto>>(active));
        }
        catch (Exception ex)
        {
            LogError(ex, "GetAllAsync: Error retrieving all templates");
            return Result<List<QuestionnaireTemplateDto>>.Error("Failed to retrieve templates.");
        }
    }

    // Get filtered templates with pagination
    public async Task<Result<Recruiter.Application.Common.Dto.PagedResult<QuestionnaireTemplateDto>>> GetFilteredAsync(QuestionnaireTemplateListQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 10;

        try
        {
            var countSpec = new QuestionnaireTemplateFilterCountSpec(query);
            var filterSpec = new QuestionnaireTemplateFilterSpec(query);

            var totalCount = await _templateRepository.CountAsync(countSpec, cancellationToken);
            var templates = await _templateRepository.ListAsync(filterSpec, cancellationToken);

            var templateDtos = _mapper.Map<List<QuestionnaireTemplateDto>>(templates);

            var result = new Recruiter.Application.Common.Dto.PagedResult<QuestionnaireTemplateDto>
            {
                Items = templateDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return Result<Recruiter.Application.Common.Dto.PagedResult<QuestionnaireTemplateDto>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<Recruiter.Application.Common.Dto.PagedResult<QuestionnaireTemplateDto>>.Error();
        }
    }

    public async Task<Result<QuestionnaireTemplateDto>> GetByIdAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionSpec(name, version), cancellationToken);
            
            if (entity == null)
                return Result<QuestionnaireTemplateDto>.NotFound();

            return Result<QuestionnaireTemplateDto>.Success(_mapper.Map<QuestionnaireTemplateDto>(entity));
        }
        catch (ArgumentNullException ex)
        {
            LogError(ex, $"GetByIdAsync: Invalid parameters: Name={name}, Version={version}");
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetByIdAsync: Error retrieving template: Name={name}, Version={version}");
            return Result<QuestionnaireTemplateDto>.Error("Failed to retrieve template.");
        }
    }

    public async Task<QuestionnaireTemplateDto?> GetByIdNullableAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionSpec(name, version), cancellationToken);
            return entity != null ? _mapper.Map<QuestionnaireTemplateDto>(entity) : null;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetByIdAsync: Error retrieving template: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<QuestionnaireTemplateDto?> GetLatestVersionAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateLatestByNameSpec(name), cancellationToken);
            return entity != null ? _mapper.Map<QuestionnaireTemplateDto>(entity) : null;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetLatestVersionAsync: Error retrieving latest template version: Name={name}");
            throw;
        }
    }

    public async Task<IEnumerable<QuestionnaireTemplateDto>> GetAllVersionsAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _templateRepository.ListAsync(
                new QuestionnaireTemplatesByNameSpec(name), cancellationToken);
            return _mapper.Map<IEnumerable<QuestionnaireTemplateDto>>(entities);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetAllVersionsAsync: Error retrieving all template versions: Name={name}");
            throw;
        }
    }

    public async Task<bool> TemplateExistsAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.AnyAsync(
                new QuestionnaireTemplateExistsByNameAndVersionSpec(name, version), cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"TemplateExistsAsync: Error checking template existence: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<bool> TemplateNameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.AnyAsync(
                new QuestionnaireTemplatesByNameSpec(name), cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"TemplateNameExistsAsync: Error checking template name existence: Name={name}");
            throw;
        }
    }

    public async Task<Domain.Models.QuestionnaireTemplate?> GetTemplateForUpdateAsync(
        string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionSpec(name, version), cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForUpdateAsync: Error retrieving template for update: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<Domain.Models.QuestionnaireTemplate?> GetTemplateForQuestionActivationAsync(
        string templateName, int templateVersion, int sectionOrder, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionForQuestionActivationSpec(templateName, templateVersion, sectionOrder),
                cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForQuestionActivationAsync: Error retrieving template for question activation: Name={templateName}, Version={templateVersion}, SectionOrder={sectionOrder}");
            throw;
        }
    }

    public async Task<Domain.Models.QuestionnaireTemplate?> GetTemplateForDeleteAsync(
        string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionSpec(name, version), cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForDeleteAsync: Error retrieving template for delete: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<Domain.Models.QuestionnaireTemplate?> GetTemplateForRestoreAsync(
        string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionAnySpec(name, version), cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForRestoreAsync: Error retrieving template for restore: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<Domain.Models.QuestionnaireTemplate?> GetTemplateForPublishAsync(
        string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionSpec(name, version), cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForPublishAsync: Error retrieving template for publish: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<Domain.Models.QuestionnaireTemplate?> GetTemplateForCandidateAsync(
        string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionSpec(name, version), cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForCandidateAsync: Error retrieving template for candidate: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<Domain.Models.QuestionnaireTemplate?> GetTemplateForDuplicateAsync(
        string sourceName, int sourceVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionSpec(sourceName, sourceVersion), cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateForDuplicateAsync: Error retrieving template for duplicate: Name={sourceName}, Version={sourceVersion}");
            throw;
        }
    }

    public async Task<List<VersionHistoryItemDto>> GetTemplateVersionHistoryAsync(
        string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = await _templateRepository.ListAsync(
                new QuestionnaireTemplatesByNameSpec(name), cancellationToken);
            return templates
                .OrderByDescending(t => t.Version)
                .Select(t => new VersionHistoryItemDto
                {
                    Version = t.Version,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    CreatedBy = t.CreatedBy,
                    UpdatedBy = t.UpdatedBy,
                    IsDeleted = t.IsDeleted
                })
                .ToList();
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetTemplateVersionHistoryAsync: Error retrieving template version history: Name={name}");
            throw;
        }
    }

    public async Task<QuestionnaireQuestionDto?> GetQuestionVersionAsync(
        string questionName, int questionVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _questionRepository.FirstOrDefaultAsync(
                new QuestionByNameAndVersionWithOptionsSpec(questionName, questionVersion),
                cancellationToken);
            return entity != null ? _mapper.Map<QuestionnaireQuestionDto>(entity) : null;
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetQuestionVersionAsync: Error retrieving question version: Name={questionName}, Version={questionVersion}");
            throw;
        }
    }

    public async Task<List<QuestionnaireQuestionHistoryDetailsDto>> GetQuestionHistoryWithOptionsAsync(
        string questionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _questionRepository.ListAsync(
                new QuestionsByNameWithOptionsSpec(questionName), cancellationToken);
            return _mapper.Map<List<QuestionnaireQuestionHistoryDetailsDto>>(entities);
        }
        catch (Exception ex)
        {
            LogError(ex, $"GetQuestionHistoryWithOptionsAsync: Error retrieving question history: Name={questionName}");
            throw;
        }
    }

    public async Task<bool> IsTemplateInUseBySubmissionsAsync(
        string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            var submissionCount = await _submissionRepository.CountAsync(
                new TemplateInUseBySubmissionsSpec(name, version), cancellationToken);
            return submissionCount > 0;
        }
        catch (Exception ex)
        {
            LogError(ex, $"IsTemplateInUseBySubmissionsAsync: Error checking if template is in use by submissions: Name={name}, Version={version}");
            throw;
        }
    }

    public async Task<bool> IsTemplateInUseByJobStepsAsync(
        string name, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            var jobStepCount = await _jobPostStepRepository.CountAsync(
                new JobPostStepsUsingAssessmentTemplateSpec(name, version), cancellationToken);
            return jobStepCount > 0;
        }
        catch (Exception ex)
        {
            LogError(ex, $"IsTemplateInUseByJobStepsAsync: Error checking if template is in use by job steps: Name={name}, Version={version}");
            throw;
        }
    }

    private void LogError(Exception ex, string message)
    {
        _logger.LogError(ex, message);
    }
}


