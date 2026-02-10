using System.Globalization;
using Ardalis.Specification;
using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.Common.Helpers;
using Recruiter.Application.QuestionnaireTemplate.Dto;
using Recruiter.Application.QuestionnaireTemplate.Import.Dto;
using Recruiter.Application.QuestionnaireTemplate.Import.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Import.SpreadsheetMl;
using Recruiter.Application.QuestionnaireTemplate.Interfaces;
using Recruiter.Application.QuestionnaireTemplate.Specifications;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.Application.QuestionnaireTemplate.Import;

public sealed class QuestionnaireTemplateImportService : IQuestionnaireTemplateImportService
{
    private const string ImportWorksheetName = "Import";

    private static readonly HashSet<string> AllowedTemplateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Quiz", "Personality", "Form"
    };

    private static readonly HashSet<string> AllowedQuestionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Text", "Textarea", "Radio", "Checkbox", "Dropdown",
        "SingleChoice", "MultiChoice",
        "Likert"
    };

    /// <summary>Quiz allows only scored choice types. Personality allows only Likert. Form allows all.</summary>
    private static readonly HashSet<string> QuizQuestionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "SingleChoice", "MultiChoice"
    };

    private static readonly HashSet<string> PersonalityQuestionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Likert"
    };

    private readonly IRepository<Domain.Models.QuestionnaireTemplate> _templateRepository;
    private readonly IRepository<QuestionnaireCandidateSubmission> _submissionRepository;
    private readonly IQuestionnaireTemplateService _templateService;
    private readonly IMapper _mapper;

    public QuestionnaireTemplateImportService(
        IRepository<Domain.Models.QuestionnaireTemplate> templateRepository,
        IRepository<QuestionnaireCandidateSubmission> submissionRepository,
        IQuestionnaireTemplateService templateService,
        IMapper mapper)
    {
        _templateRepository = templateRepository;
        _submissionRepository = submissionRepository;
        _templateService = templateService;
        _mapper = mapper;
    }

    public async Task<QuestionnaireTemplateImportValidationResultDto> ValidateAsync(
        Stream spreadsheetStream,
        QuestionnaireTemplateImportRequestDto? request = null,
        CancellationToken cancellationToken = default)
    {
        var rowsByHeader = SpreadsheetMlReader.ReadWorksheetRowsByHeader(spreadsheetStream, ImportWorksheetName);

        var rows = rowsByHeader
            .Select((dict, idx) => MapRow(dict, rowNumber: idx + 3)) // +3 because data starts on Excel row 3
            .ToList();

        ApplyOverrides(rows, request);
        ApplyCarryForwardDefaults(rows);

        var errors = new List<QuestionnaireTemplateImportValidationErrorDto>();
        if (rows.Count == 0)
        {
            errors.Add(new QuestionnaireTemplateImportValidationErrorDto
            {
                RowNumber = 1,
                Column = null,
                Message = "The import sheet contains no data rows.",
                Severity = "Error"
            });

            return new QuestionnaireTemplateImportValidationResultDto
            {
                IsValid = false,
                Errors = errors
            };
        }

        // Frontend provides import context (scope/template selection).
        // The spreadsheet should primarily contain section/question/option content.
        // For backward compatibility, fall back to spreadsheet values if request values are not provided.
        var templateName = string.IsNullOrWhiteSpace(request?.TemplateName)
            ? FirstNonEmpty(rows.Select(r => r.TemplateName))
            : request!.TemplateName!.Trim();

        var templateType = string.IsNullOrWhiteSpace(request?.TemplateType)
            ? FirstNonEmpty(rows.Select(r => r.TemplateType))
            : request!.TemplateType!.Trim();

        var scopeRaw = request?.Scope?.ToString() ?? FirstNonEmpty(rows.Select(r => r.Scope));
        var requestedTemplateVersion = request?.TemplateVersion;

        var scope = TryParseScope(scopeRaw, out var scopeParsed) ? scopeParsed : (QuestionnaireTemplateImportScope?)null;
        if (scope == null)
        {
            errors.Add(new QuestionnaireTemplateImportValidationErrorDto
            {
                RowNumber = rows.First().RowNumber,
                Column = "Scope",
                Message = "Scope is required and must be CreateTemplate, AppendToTemplate, or AppendToSection.",
                Severity = "Error"
            });
        }

        if (string.IsNullOrWhiteSpace(templateName))
        {
            errors.Add(new QuestionnaireTemplateImportValidationErrorDto
            {
                RowNumber = rows.First().RowNumber,
                Column = "TemplateName",
                Message = "TemplateName is required.",
                Severity = "Error"
            });
        }

        // Target section order comes from the request for AppendToSection; sheet fallback kept for backward compatibility.
        var targetSectionOrderRaw = request?.TargetSectionOrder.HasValue == true
            ? request.TargetSectionOrder.Value.ToString(CultureInfo.InvariantCulture)
            : FirstNonEmpty(rows.Select(r => r.TargetSectionOrder));

        // TemplateType requirements:
        // - CreateTemplate: required and must be valid
        // - Append scopes: may be omitted by the UI (we can infer from the existing template once loaded)
        if (!string.IsNullOrWhiteSpace(templateType) && !AllowedTemplateTypes.Contains(templateType))
        {
            errors.Add(new QuestionnaireTemplateImportValidationErrorDto
            {
                RowNumber = rows.First().RowNumber,
                Column = "TemplateType",
                Message = "TemplateType must be Quiz, Personality, or Form.",
                Severity = "Error"
            });
        }
        else if (string.IsNullOrWhiteSpace(templateType) && scope == QuestionnaireTemplateImportScope.CreateTemplate)
        {
            errors.Add(new QuestionnaireTemplateImportValidationErrorDto
            {
                RowNumber = rows.First().RowNumber,
                Column = "TemplateType",
                Message = "TemplateType is required and must be Quiz, Personality, or Form.",
                Severity = "Error"
            });
        }

        // Load existing template context early for better validation/UX.
        Domain.Models.QuestionnaireTemplate? existingTemplateForAppend = null;
        if (scope is QuestionnaireTemplateImportScope.AppendToTemplate or QuestionnaireTemplateImportScope.AppendToSection)
        {
            if (!string.IsNullOrWhiteSpace(templateName) && requestedTemplateVersion is > 0)
            {
                existingTemplateForAppend = await _templateRepository.FirstOrDefaultAsync(
                    new QuestionnaireTemplateByNameAndVersionSpec(templateName.Trim(), requestedTemplateVersion.Value),
                    cancellationToken);

                if (existingTemplateForAppend == null)
                {
                    errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                    {
                        RowNumber = rows.First().RowNumber,
                        Column = "TemplateVersion",
                        Message = $"Template '{templateName}' v{requestedTemplateVersion.Value} was not found.",
                        Severity = "Error"
                    });
                }
                else
                {
                    // For append imports, we can infer TemplateType from the existing template if the UI didn't provide it.
                    if (string.IsNullOrWhiteSpace(templateType))
                        templateType = existingTemplateForAppend.TemplateType.ToString();

                    // For append imports, TemplateType must match the existing template type (prevents misleading validation + accidental misuse).
                    if (!string.IsNullOrWhiteSpace(templateType)
                        && !string.Equals(existingTemplateForAppend.TemplateType.ToString(), templateType.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                        {
                            RowNumber = rows.First().RowNumber,
                            Column = "TemplateType",
                            Message = $"TemplateType '{templateType}' does not match existing template type '{existingTemplateForAppend.TemplateType}'.",
                            Severity = "Error"
                        });
                    }

                    if (scope == QuestionnaireTemplateImportScope.AppendToSection
                        && TryParsePositiveInt(targetSectionOrderRaw, out var targetSectionOrder))
                    {
                        var hasSection = existingTemplateForAppend.Sections.Any(s => !s.IsDeleted && s.Order == targetSectionOrder);
                        if (!hasSection)
                        {
                            errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                            {
                                RowNumber = rows.First().RowNumber,
                                Column = "TargetSectionOrder",
                                Message = $"Section order {targetSectionOrder} was not found in template '{templateName}' v{requestedTemplateVersion.Value}.",
                                Severity = "Error"
                            });
                        }
                    }
                }
            }
        }

        // Append imports should target a specific existing template version (selected in UI).
        if (scope is QuestionnaireTemplateImportScope.AppendToTemplate or QuestionnaireTemplateImportScope.AppendToSection)
        {
            if (!requestedTemplateVersion.HasValue || requestedTemplateVersion.Value <= 0)
            {
                errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                {
                    RowNumber = rows.First().RowNumber,
                    Column = "TemplateVersion",
                    Message = "TemplateVersion is required for append imports.",
                    Severity = "Error"
                });
            }
        }

        // Validate scope-specific requirements (TargetSectionOrder for AppendToSection)
        if (scope == QuestionnaireTemplateImportScope.AppendToSection)
        {
            if (!TryParsePositiveInt(targetSectionOrderRaw, out _))
            {
                errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                {
                    RowNumber = rows.First().RowNumber,
                    Column = "TargetSectionOrder",
                    Message = "TargetSectionOrder is required for AppendToSection and must be a positive integer.",
                    Severity = "Error"
                });
            }
        }

        // Validate each row and compute counts.
        var sectionKeys = new HashSet<int>();
        var questionKeys = new HashSet<(int SectionOrder, int QuestionOrder)>();
        var optionCount = 0;
        var seenSectionTitleForCreate = new HashSet<int>();

        // Duplicate detection to help UX (non-blocking warnings):
        // - prompt duplicates within the same section, across DIFFERENT questions (options rows repeat the same prompt and are expected)
        //
        // Key idea: a "question" is identified by (SectionOrder, QuestionOrder). Multiple rows may exist for one question (one per option).
        var seenQuestionPrompt = new Dictionary<(int SectionOrder, int QuestionOrder), string>(capacity: 32);
        var seenPromptBySection = new Dictionary<int, HashSet<string>>(capacity: 8);
        Dictionary<int, HashSet<string>>? existingPromptsBySection = null;
        if (existingTemplateForAppend != null)
        {
            existingPromptsBySection = existingTemplateForAppend.Sections
                .Where(s => !s.IsDeleted)
                .ToDictionary(
                    s => s.Order,
                    s => new HashSet<string>(
                        s.Questions
                            .Where(q => q.IsActive)
                            .Select(q => (q.QuestionText ?? string.Empty).Trim())
                            .Where(t => !string.IsNullOrWhiteSpace(t)),
                        StringComparer.OrdinalIgnoreCase));
        }

        foreach (var row in rows)
        {
            // For AppendToSection, section is selected via TargetSectionOrder (SectionOrder/SectionTitle are ignored).
            // For other scopes, SectionOrder is required; SectionTitle is required when creating a section.
            var isAppendToSection = scope == QuestionnaireTemplateImportScope.AppendToSection;
            int sectionOrder;
            if (isAppendToSection)
            {
                if (!TryParsePositiveInt(targetSectionOrderRaw, out sectionOrder))
                {
                    // TargetSectionOrder error is already captured above; skip row-level section validations.
                    continue;
                }

                // In AppendToSection, we only import rows that belong to the selected section.
                // Users often keep other sections in the spreadsheet; those must be ignored, not remapped.
                // If SectionOrder is blank, assume the row belongs to the selected target section.
                // This keeps the UX simple when users keep ONLY one section's rows in the sheet.
                // If the sheet contains multiple sections, non-target rows should explicitly set SectionOrder
                // so they can be ignored.
                var hasRowSection = TryParsePositiveInt(row.SectionOrder, out var rowSectionOrder);
                if (!hasRowSection)
                {
                    rowSectionOrder = sectionOrder;
                }

                if (rowSectionOrder != sectionOrder)
                {
                    errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                    {
                        RowNumber = row.RowNumber,
                        Column = "SectionOrder",
                        Message = $"Row will be ignored because SectionOrder={rowSectionOrder} does not match TargetSectionOrder={sectionOrder}.",
                        Severity = "Warning"
                    });
                    continue;
                }
            }
            else
            {
                if (!TryParsePositiveInt(row.SectionOrder, out sectionOrder))
                {
                    errors.Add(Err(row, "SectionOrder", "SectionOrder is required and must be a positive integer."));
                    continue;
                }

                // For CreateTemplate we always create sections; for AppendToTemplate, section title is required only when adding a NEW section.
                if (scope == QuestionnaireTemplateImportScope.CreateTemplate)
                {
                    // Require SectionTitle only the first time a section order appears in the file.
                    // This keeps spreadsheets clean: subsequent rows within the same section can leave SectionTitle blank.
                    if (!seenSectionTitleForCreate.Contains(sectionOrder))
                    {
                        if (string.IsNullOrWhiteSpace(row.SectionTitle))
                            errors.Add(Err(row, "SectionTitle", "SectionTitle is required for the first row of a section."));
                        else
                            seenSectionTitleForCreate.Add(sectionOrder);
                    }
                }
                else if (scope == QuestionnaireTemplateImportScope.AppendToTemplate && existingTemplateForAppend != null)
                {
                    var sectionExists = existingTemplateForAppend.Sections.Any(s => !s.IsDeleted && s.Order == sectionOrder);
                    if (!sectionExists && string.IsNullOrWhiteSpace(row.SectionTitle))
                        errors.Add(Err(row, "SectionTitle", "SectionTitle is required when adding a new section."));
                }
                else
                {
                    // If we don't know the existing template graph yet, keep the original stricter behavior.
                    if (scope == QuestionnaireTemplateImportScope.AppendToTemplate && string.IsNullOrWhiteSpace(row.SectionTitle))
                        errors.Add(Err(row, "SectionTitle", "SectionTitle is required."));
                }
            }

            if (!TryParsePositiveInt(row.QuestionOrder, out var questionOrder))
            {
                errors.Add(Err(row, "QuestionOrder", "QuestionOrder is required and must be a positive integer."));
                continue;
            }

            var questionTypeRaw = row.QuestionType?.Trim();
            if (string.IsNullOrWhiteSpace(questionTypeRaw) || !AllowedQuestionTypes.Contains(questionTypeRaw))
            {
                errors.Add(Err(row, "QuestionType", "QuestionType is required and must be a valid questionnaire question type."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.QuestionTitle))
            {
                errors.Add(Err(row, "QuestionTitle", "QuestionTitle is required."));
            }
            else
            {
                var prompt = row.QuestionTitle.Trim();
                var questionKey = (SectionOrder: sectionOrder, QuestionOrder: questionOrder);
                if (!seenQuestionPrompt.TryGetValue(questionKey, out var firstPromptForThisQuestion))
                {
                    // First time we see this question in the file: record its prompt and perform duplicate checks.
                    seenQuestionPrompt[questionKey] = prompt;

                    if (!seenPromptBySection.TryGetValue(sectionOrder, out var prompts))
                    {
                        prompts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        seenPromptBySection[sectionOrder] = prompts;
                    }

                    // Warn on duplicates within the import file for the same section across DIFFERENT questions.
                    if (!prompts.Add(prompt))
                    {
                        errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                        {
                            RowNumber = row.RowNumber,
                            Column = "QuestionTitle",
                            Message = $"Duplicate question prompt detected in section {sectionOrder}. It will be skipped during import: '{prompt}'.",
                            Severity = "Warning"
                        });
                    }
                }
                else
                {
                    // Same question repeated (options rows). Allow the same prompt; warn if the prompt changes within the same question.
                    if (!string.Equals(firstPromptForThisQuestion, prompt, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                        {
                            RowNumber = row.RowNumber,
                            Column = "QuestionTitle",
                            Message = $"QuestionTitle differs across rows for Section {sectionOrder}, Question {questionOrder}. Keep it consistent (one question = one title).",
                            Severity = "Error"
                        });
                    }
                }

                // Warn on duplicates against existing template prompts for append scopes (new questions should be skipped).
                // Only warn once per question (not per option row).
                if (existingPromptsBySection != null &&
                    !string.IsNullOrWhiteSpace(prompt) &&
                    existingPromptsBySection.TryGetValue(sectionOrder, out var existingPrompts) &&
                    existingPrompts.Contains(prompt) &&
                    seenQuestionPrompt.ContainsKey((sectionOrder, questionOrder)) &&
                    string.Equals(seenQuestionPrompt[(sectionOrder, questionOrder)], prompt, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                    {
                        RowNumber = row.RowNumber,
                        Column = "QuestionTitle",
                        Message = $"Duplicate question prompt already exists in target template section {sectionOrder}. It will be skipped: '{prompt}'.",
                        Severity = "Warning"
                    });
                }
            }

            // TemplateType + QuestionType compatibility: Quiz = SingleChoice/MultiChoice only; Personality = Likert only; Form = any.
            if (!string.IsNullOrWhiteSpace(templateType))
            {
                if (string.Equals(templateType, "Quiz", StringComparison.OrdinalIgnoreCase)
                    && !QuizQuestionTypes.Contains(questionTypeRaw))
                {
                    errors.Add(Err(row, "QuestionType",
                        $"Template type is not correct: Quiz allows only SingleChoice and MultiChoice. This question has type '{questionTypeRaw}'."));
                }
                else if (string.Equals(templateType, "Personality", StringComparison.OrdinalIgnoreCase)
                    && !PersonalityQuestionTypes.Contains(questionTypeRaw))
                {
                    errors.Add(Err(row, "QuestionType",
                        $"Template type is not correct: Personality templates allow only Likert questions. This question has type '{questionTypeRaw}'."));
                }
            }

            var isLikert = string.Equals(questionTypeRaw, "Likert", StringComparison.OrdinalIgnoreCase);
            if (isLikert)
            {
                if (string.IsNullOrWhiteSpace(row.TraitKey))
                    errors.Add(Err(row, "TraitKey", "TraitKey is required for Likert questions."));
                if (!TryParseDecimal(row.Ws, out _))
                    errors.Add(Err(row, "Ws", "Ws is required for Likert questions and must be a number."));
            }

            // Option-based types require at least one option row. We validate per-row option presence.
            var isOptionBased = IsOptionBasedType(questionTypeRaw);
            if (isOptionBased)
            {
                if (!TryParsePositiveInt(row.OptionOrder, out var optionOrder))
                {
                    errors.Add(Err(row, "OptionOrder", "OptionOrder is required for option-based question types and must be a positive integer."));
                }
                else
                {
                    optionCount++;

                    if (string.IsNullOrWhiteSpace(row.OptionLabel))
                        errors.Add(Err(row, "OptionLabel", "OptionLabel is required for option-based question types."));

                    // Likert expects Wa, Quiz can use IsCorrect/Score. Keep validations minimal but correct.
                    if (isLikert && !TryParseDecimal(row.Wa, out _))
                        errors.Add(Err(row, "Wa", "Wa is required for Likert options and must be a number."));
                }
            }
            else
            {
                // Non-option questions should not specify option fields.
                if (!string.IsNullOrWhiteSpace(row.OptionOrder)
                    || !string.IsNullOrWhiteSpace(row.OptionLabel)
                    || !string.IsNullOrWhiteSpace(row.Score)
                    || !string.IsNullOrWhiteSpace(row.Wa)
                    || !string.IsNullOrWhiteSpace(row.IsCorrect))
                {
                    errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                    {
                        RowNumber = row.RowNumber,
                        Column = "OptionOrder",
                        Message = "This question type does not support options. Remove option columns for this row.",
                        Severity = "Warning"
                    });
                }
            }

            sectionKeys.Add(sectionOrder);
            questionKeys.Add((sectionOrder, questionOrder));
        }

        // Existing template checks (helps UX; does not block by itself here)
        var templateExists = false;
        int? existingLatestVersion = null;
        var existingLatestInUse = false;
        if (!string.IsNullOrWhiteSpace(templateName))
        {
            templateName = templateName.Trim();
            templateExists = await _templateRepository.AnyAsync(new QuestionnaireTemplatesByNameSpec(templateName), cancellationToken);
            if (templateExists)
            {
                var latest = await _templateRepository.FirstOrDefaultAsync(new QuestionnaireTemplateLatestByNameSpec(templateName), cancellationToken);
                existingLatestVersion = latest?.Version;

                if (existingLatestVersion.HasValue)
                {
                    var submissionCount = await _submissionRepository.CountAsync(
                        new TemplateInUseBySubmissionsSpec(templateName, existingLatestVersion.Value),
                        cancellationToken);
                    existingLatestInUse = submissionCount > 0;
                }

                if (scope == QuestionnaireTemplateImportScope.CreateTemplate)
                {
                    errors.Add(new QuestionnaireTemplateImportValidationErrorDto
                    {
                        RowNumber = rows.First().RowNumber,
                        Column = "TemplateName",
                        Message = $"Template '{templateName}' already exists. Use AppendToTemplate / AppendToSection instead.",
                        Severity = "Error"
                    });
                }
            }
        }

        var isValid = errors.All(e => !string.Equals(e.Severity, "Error", StringComparison.OrdinalIgnoreCase));
        return new QuestionnaireTemplateImportValidationResultDto
        {
            IsValid = isValid,
            Scope = scope,
            TemplateName = templateName,
            TemplateType = templateType,
            TemplateExists = templateExists,
            ExistingLatestVersion = existingLatestVersion,
            ExistingLatestInUse = existingLatestInUse,
            TotalRows = rows.Count,
            SectionsCount = sectionKeys.Count,
            QuestionsCount = questionKeys.Count,
            OptionsCount = optionCount,
            Errors = errors
        };
    }

    public async Task<QuestionnaireTemplateImportExecuteResultDto> ExecuteAsync(
        Stream spreadsheetStream,
        QuestionnaireTemplateImportRequestDto? request = null,
        CancellationToken cancellationToken = default)
    {
        spreadsheetStream.Position = 0;
        var validation = await ValidateAsync(spreadsheetStream, request, cancellationToken);
        if (!validation.IsValid || validation.Scope == null || string.IsNullOrWhiteSpace(validation.TemplateName) || string.IsNullOrWhiteSpace(validation.TemplateType))
        {
            var message = validation.Errors.Count > 0
                ? string.Join("; ", validation.Errors.Where(e => e.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)).Select(e => $"Row {e.RowNumber}: {e.Message}"))
                : "Import file is invalid.";
            throw new InvalidOperationException(message);
        }

        spreadsheetStream.Position = 0;
        var rowsByHeader = SpreadsheetMlReader.ReadWorksheetRowsByHeader(spreadsheetStream, ImportWorksheetName);
        var rows = rowsByHeader
            .Select((dict, idx) => MapRow(dict, rowNumber: idx + 3))
            .ToList();

        ApplyOverrides(rows, request);
        ApplyCarryForwardDefaults(rows);

        var scope = validation.Scope.Value;
        var templateName = validation.TemplateName.Trim();
        var templateType = validation.TemplateType.Trim();
        var messages = new List<QuestionnaireTemplateImportValidationErrorDto>();

        QuestionnaireTemplateDto dto;
        var createdNewVersion = false;

        if (scope == QuestionnaireTemplateImportScope.CreateTemplate)
        {
            dto = BuildNewTemplateDto(templateName, templateType, rows, request, messages);
            var created = await _templateService.CreateAsync(dto, cancellationToken);

            return new QuestionnaireTemplateImportExecuteResultDto
            {
                TemplateName = created.Name,
                TemplateVersion = created.Version,
                TemplateType = created.TemplateType,
                Scope = scope,
                CreatedNewTemplate = true,
                CreatedNewVersion = false,
                SectionsCount = created.Sections?.Count ?? 0,
                QuestionsCount = created.Sections?.SelectMany(s => s.Questions ?? []).Count() ?? 0,
                OptionsCount = created.Sections?.SelectMany(s => s.Questions ?? []).SelectMany(q => q.Options ?? []).Count() ?? 0,
                Messages = messages
            };
        }

        Domain.Models.QuestionnaireTemplate? targetEntity;
        if (request?.TemplateVersion is > 0)
        {
            targetEntity = await _templateRepository.FirstOrDefaultAsync(
                new QuestionnaireTemplateByNameAndVersionSpec(templateName, request.TemplateVersion.Value),
                cancellationToken);
        }
        else
        {
            targetEntity = await _templateRepository.FirstOrDefaultAsync(new QuestionnaireTemplateLatestByNameSpec(templateName), cancellationToken);
        }

        if (targetEntity == null)
            throw new InvalidOperationException($"Template '{templateName}' was not found.");

        var latestDto = _mapper.Map<QuestionnaireTemplateDto>(targetEntity);

        var inUseCount = await _submissionRepository.CountAsync(
            new TemplateInUseBySubmissionsSpec(targetEntity.Name, targetEntity.Version),
            cancellationToken);
        var isInUse = inUseCount > 0;
        if (isInUse)
        {
            latestDto.ShouldUpdateVersion = true;
            createdNewVersion = true;
        }

        ApplyImportToTemplateDto(latestDto, templateType, scope, rows, messages);

        var updated = await _templateService.UpdateAsync(latestDto, cancellationToken);

        return new QuestionnaireTemplateImportExecuteResultDto
        {
            TemplateName = updated.Name,
            TemplateVersion = updated.Version,
            TemplateType = updated.TemplateType,
            Scope = scope,
            CreatedNewTemplate = false,
            CreatedNewVersion = createdNewVersion,
            SectionsCount = updated.Sections?.Count ?? 0,
            QuestionsCount = updated.Sections?.SelectMany(s => s.Questions ?? []).Count() ?? 0,
            OptionsCount = updated.Sections?.SelectMany(s => s.Questions ?? []).SelectMany(q => q.Options ?? []).Count() ?? 0,
            Messages = messages
        };
    }

    private static QuestionnaireTemplateDto BuildNewTemplateDto(
        string templateName,
        string templateType,
        List<QuestionnaireTemplateImportRowDto> rows,
        QuestionnaireTemplateImportRequestDto? request,
        List<QuestionnaireTemplateImportValidationErrorDto> messages)
    {
        var title = request?.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
            title = FirstNonEmpty(rows.Select(r => r.Title));

        var description = request?.Description?.Trim();
        if (string.IsNullOrWhiteSpace(description))
            description = FirstNonEmpty(rows.Select(r => r.Description));

        // TimeLimitSeconds is not used currently.
        int? timeLimit = null;

        var dto = new QuestionnaireTemplateDto
        {
            Name = templateName,
            Version = 1,
            TemplateType = templateType,
            Status = "Draft",
            Title = title,
            Description = description,
            TimeLimitSeconds = timeLimit,
            Sections = new List<QuestionnaireSectionDto>()
        };

        ApplyImportToTemplateDto(dto, templateType, QuestionnaireTemplateImportScope.CreateTemplate, rows, messages);
        return dto;
    }

    private static void ApplyOverrides(List<QuestionnaireTemplateImportRowDto> rows, QuestionnaireTemplateImportRequestDto? request)
    {
        if (request == null)
            return;

        var scope = request.Scope?.ToString();
        foreach (var r in rows)
        {
            if (!string.IsNullOrWhiteSpace(scope))
                r.Scope = scope;
            if (!string.IsNullOrWhiteSpace(request.TemplateName))
                r.TemplateName = request.TemplateName.Trim();
            if (!string.IsNullOrWhiteSpace(request.TemplateType))
                r.TemplateType = request.TemplateType.Trim();
            if (request.TargetSectionOrder.HasValue)
                r.TargetSectionOrder = request.TargetSectionOrder.Value.ToString();
        }
    }

    private static void ApplyCarryForwardDefaults(List<QuestionnaireTemplateImportRowDto> rows)
    {
        // Make the spreadsheet easier to fill:
        // Users can leave repeated values blank to mean "same as previous row" (especially for multi-option questions).
        QuestionnaireTemplateImportRowDto? prev = null;
        foreach (var r in rows)
        {
            if (prev == null)
            {
                prev = r;
                continue;
            }

            // Template-level carry-forward
            if (string.IsNullOrWhiteSpace(r.Scope)) r.Scope = prev.Scope;
            if (string.IsNullOrWhiteSpace(r.TemplateName)) r.TemplateName = prev.TemplateName;
            if (string.IsNullOrWhiteSpace(r.TemplateType)) r.TemplateType = prev.TemplateType;
            if (string.IsNullOrWhiteSpace(r.Title)) r.Title = prev.Title;
            if (string.IsNullOrWhiteSpace(r.Description)) r.Description = prev.Description;
            if (string.IsNullOrWhiteSpace(r.TargetSectionOrder)) r.TargetSectionOrder = prev.TargetSectionOrder;

            // Section-level carry-forward:
            // Most spreadsheets keep SectionOrder only on the first row for a section, and leave it blank for subsequent questions.
            // We should treat blank SectionOrder as "same section as previous row" when the row is clearly continuing the same section.
            // Many users repeat SectionTitle on every row; allow that too as long as the title matches the previous row.
            // This prevents accidentally masking a missing SectionOrder on a new section header row (title differs).
            var hasAnyQuestionOrOptionData =
                !string.IsNullOrWhiteSpace(r.QuestionOrder) ||
                !string.IsNullOrWhiteSpace(r.QuestionType) ||
                !string.IsNullOrWhiteSpace(r.IsRequired) ||
                !string.IsNullOrWhiteSpace(r.QuestionTitle) ||
                !string.IsNullOrWhiteSpace(r.TraitKey) ||
                !string.IsNullOrWhiteSpace(r.Ws) ||
                !string.IsNullOrWhiteSpace(r.OptionOrder) ||
                !string.IsNullOrWhiteSpace(r.OptionLabel) ||
                !string.IsNullOrWhiteSpace(r.Score) ||
                !string.IsNullOrWhiteSpace(r.Wa) ||
                !string.IsNullOrWhiteSpace(r.IsCorrect);

            if (hasAnyQuestionOrOptionData &&
                string.IsNullOrWhiteSpace(r.SectionOrder) &&
                (string.IsNullOrWhiteSpace(r.SectionTitle) ||
                 (!string.IsNullOrWhiteSpace(prev.SectionTitle) &&
                  string.Equals(r.SectionTitle.Trim(), prev.SectionTitle.Trim(), StringComparison.OrdinalIgnoreCase))))
            {
                r.SectionOrder = prev.SectionOrder;
            }

            // Section/question carry-forward (so option rows can be minimal)
            // Only apply this for option-rows (OptionOrder present). This prevents accidentally carrying forward
            // question identity into the next question when the user forgets to fill required fields.
            var isOptionRow = !string.IsNullOrWhiteSpace(r.OptionOrder) ||
                              !string.IsNullOrWhiteSpace(r.OptionLabel) ||
                              !string.IsNullOrWhiteSpace(r.Score) ||
                              !string.IsNullOrWhiteSpace(r.Wa) ||
                              !string.IsNullOrWhiteSpace(r.IsCorrect);

            if (isOptionRow)
            {
                // IMPORTANT:
                // We only carry forward question identity fields for option rows AFTER the first option row.
                // This keeps the spreadsheet user-friendly (option rows can be blank) while avoiding a common mistake:
                // starting a new question but forgetting to set QuestionOrder/QuestionTitle on the first option row,
                // which would otherwise inherit values from the previous question and produce confusing validation errors.
                var optionOrder = 0;
                var hasOptionOrder = TryParsePositiveInt(r.OptionOrder, out optionOrder);
                var canCarryQuestionIdentity = hasOptionOrder && optionOrder > 1;

                if (canCarryQuestionIdentity)
                {
                    if (string.IsNullOrWhiteSpace(r.SectionOrder)) r.SectionOrder = prev.SectionOrder;
                    if (string.IsNullOrWhiteSpace(r.SectionTitle)) r.SectionTitle = prev.SectionTitle;
                    if (string.IsNullOrWhiteSpace(r.QuestionOrder)) r.QuestionOrder = prev.QuestionOrder;
                    if (string.IsNullOrWhiteSpace(r.QuestionType)) r.QuestionType = prev.QuestionType;
                    if (string.IsNullOrWhiteSpace(r.IsRequired)) r.IsRequired = prev.IsRequired;
                    if (string.IsNullOrWhiteSpace(r.QuestionTitle)) r.QuestionTitle = prev.QuestionTitle;
                    if (string.IsNullOrWhiteSpace(r.TraitKey)) r.TraitKey = prev.TraitKey;
                    if (string.IsNullOrWhiteSpace(r.Ws)) r.Ws = prev.Ws;
                }
            }

            prev = r;
        }
    }

    private static void ApplyImportToTemplateDto(
        QuestionnaireTemplateDto target,
        string templateType,
        QuestionnaireTemplateImportScope scope,
        List<QuestionnaireTemplateImportRowDto> rows,
        List<QuestionnaireTemplateImportValidationErrorDto>? messages)
    {
        var targetSectionOrder = scope == QuestionnaireTemplateImportScope.AppendToSection
            ? (TryParsePositiveInt(FirstNonEmpty(rows.Select(r => r.TargetSectionOrder)), out var tso) ? tso : (int?)null)
            : null;

        foreach (var row in rows)
        {
            int sectionOrder;
            if (scope == QuestionnaireTemplateImportScope.AppendToSection &&
                TryParsePositiveInt(FirstNonEmpty(rows.Select(r => r.TargetSectionOrder)), out var targetOrder))
            {
                // For AppendToSection we import ONLY rows belonging to the selected section.
                // SectionOrder is used as a filter (after carry-forward), then we remap into the selected section.
                if (!TryParsePositiveInt(row.SectionOrder, out var rowSectionOrder))
                    rowSectionOrder = targetOrder;
                if (rowSectionOrder != targetOrder)
                    continue;

                sectionOrder = targetOrder;
            }
            else
            {
                if (!TryParsePositiveInt(row.SectionOrder, out sectionOrder))
                    continue;
            }

            if (targetSectionOrder.HasValue && sectionOrder != targetSectionOrder.Value)
                continue;

            if (!TryParsePositiveInt(row.QuestionOrder, out var questionOrder))
                continue;

            var section = target.Sections.FirstOrDefault(s => s.Order == sectionOrder);
            if (section == null)
            {
                section = new QuestionnaireSectionDto
                {
                    Id = Guid.NewGuid(),
                    Order = sectionOrder,
                    Title = string.IsNullOrWhiteSpace(row.SectionTitle) ? $"Section {sectionOrder}" : row.SectionTitle.Trim(),
                    Questions = new List<QuestionnaireQuestionDto>()
                };
                target.Sections.Add(section);
            }
            else if (!string.IsNullOrWhiteSpace(row.SectionTitle) && scope != QuestionnaireTemplateImportScope.AppendToSection)
            {
                section.Title = row.SectionTitle.Trim();
            }

            // Find or create question by order within section.
            var existingQuestion = (section.Questions ?? new List<QuestionnaireQuestionDto>()).FirstOrDefault(q => q.Order == questionOrder);
            if (existingQuestion == null)
            {
                var incomingPrompt = (row.QuestionTitle ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(incomingPrompt))
                {
                    var promptExists = (section.Questions ?? new List<QuestionnaireQuestionDto>())
                        .Any(q => !string.IsNullOrWhiteSpace(q.PromptText)
                                  && string.Equals(q.PromptText.Trim(), incomingPrompt, StringComparison.OrdinalIgnoreCase));
                    if (promptExists)
                    {
                        messages?.Add(new QuestionnaireTemplateImportValidationErrorDto
                        {
                            RowNumber = row.RowNumber,
                            Column = "QuestionTitle",
                            Message = $"Skipped duplicate question prompt in section {sectionOrder}: '{incomingPrompt}'.",
                            Severity = "Warning"
                        });
                        continue;
                    }
                }

                existingQuestion = new QuestionnaireQuestionDto
                {
                    Name = GenerateStableQuestionName(target.Name, target.Version, sectionOrder, questionOrder, row.QuestionTitle),
                    Version = 1,
                    Order = questionOrder,
                    QuestionType = (row.QuestionType ?? "Text").Trim(),
                    IsRequired = ParseBool(row.IsRequired),
                    PromptText = (row.QuestionTitle ?? string.Empty).Trim(),
                    TraitKey = string.IsNullOrWhiteSpace(row.TraitKey) ? null : row.TraitKey.Trim(),
                    Ws = TryParseDecimal(row.Ws, out var ws) ? ws : null,
                    Options = new List<QuestionnaireOptionDto>()
                };

                section.Questions ??= new List<QuestionnaireQuestionDto>();
                section.Questions.Add(existingQuestion);
            }
            else
            {
                // If the incoming prompt duplicates ANOTHER question prompt in the same section, skip the row.
                // This prevents accidental creation of “duplicate questions” in the UI (even though DB doesn't enforce prompt uniqueness).
                var incomingPrompt = (row.QuestionTitle ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(incomingPrompt))
                {
                    var promptUsedByOther = (section.Questions ?? new List<QuestionnaireQuestionDto>())
                        .Any(q =>
                            q.Order != existingQuestion.Order &&
                            !string.IsNullOrWhiteSpace(q.PromptText) &&
                            string.Equals(q.PromptText.Trim(), incomingPrompt, StringComparison.OrdinalIgnoreCase));
                    if (promptUsedByOther)
                    {
                        messages?.Add(new QuestionnaireTemplateImportValidationErrorDto
                        {
                            RowNumber = row.RowNumber,
                            Column = "QuestionTitle",
                            Message = $"Skipped row because question prompt duplicates another question in section {sectionOrder}: '{incomingPrompt}'.",
                            Severity = "Warning"
                        });
                        continue;
                    }
                }

                // Update question fields (name/version are identity)
                existingQuestion.QuestionType = (row.QuestionType ?? existingQuestion.QuestionType).Trim();
                existingQuestion.IsRequired = ParseBool(row.IsRequired);
                existingQuestion.PromptText = (row.QuestionTitle ?? existingQuestion.PromptText)?.Trim();
                existingQuestion.TraitKey = string.IsNullOrWhiteSpace(row.TraitKey) ? null : row.TraitKey.Trim();
                existingQuestion.Ws = TryParseDecimal(row.Ws, out var ws) ? ws : existingQuestion.Ws;
            }

            // Options (if option-based type)
            if (!IsOptionBasedType(existingQuestion.QuestionType))
                continue;

            if (!TryParsePositiveInt(row.OptionOrder, out var optionOrder))
                continue;

            existingQuestion.Options ??= new List<QuestionnaireOptionDto>();
            var opt = existingQuestion.Options.FirstOrDefault(o => o.Order == optionOrder);
            if (opt == null)
            {
                var incomingLabel = (row.OptionLabel ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(incomingLabel))
                {
                    var labelExists = existingQuestion.Options.Any(o =>
                        !string.IsNullOrWhiteSpace(o.Label) &&
                        string.Equals(o.Label.Trim(), incomingLabel, StringComparison.OrdinalIgnoreCase));
                    if (labelExists)
                    {
                        messages?.Add(new QuestionnaireTemplateImportValidationErrorDto
                        {
                            RowNumber = row.RowNumber,
                            Column = "OptionLabel",
                            Message = $"Skipped duplicate option label for section {sectionOrder}, question {questionOrder}: '{incomingLabel}'.",
                            Severity = "Warning"
                        });
                        continue;
                    }
                }

                opt = new QuestionnaireOptionDto
                {
                    Name = $"option_{optionOrder}",
                    Version = 1,
                    Order = optionOrder
                };
                existingQuestion.Options.Add(opt);
            }

            if (!string.IsNullOrWhiteSpace(row.OptionLabel))
                opt.Label = row.OptionLabel.Trim();
            opt.IsCorrect = ParseNullableBool(row.IsCorrect);
            opt.Score = TryParseDecimal(row.Score, out var score) ? score : opt.Score;
            opt.Wa = TryParseDecimal(row.Wa, out var wa) ? wa : opt.Wa;
        }

        // Keep deterministic ordering (important for UI and DB unique indexes)
        target.Sections = target.Sections.OrderBy(s => s.Order).ToList();
        foreach (var s in target.Sections)
        {
            s.Questions = (s.Questions ?? new List<QuestionnaireQuestionDto>()).OrderBy(q => q.Order).ToList();
            foreach (var q in s.Questions)
            {
                q.Options = (q.Options ?? new List<QuestionnaireOptionDto>()).OrderBy(o => o.Order).ToList();
            }
        }
    }

    private static string GenerateStableQuestionName(string templateName, int templateVersion, int sectionOrder, int questionOrder, string? promptText)
    {
        var templateSlug = StringHelper.Slugify(templateName);
        var promptSlug = StringHelper.Slugify((promptText ?? string.Empty).Trim());
        return $"{templateSlug}_v{templateVersion}_s{sectionOrder}_q{questionOrder}_{promptSlug}";
    }

    private static bool ParseBool(string? raw)
        => raw != null && (raw.Equals("true", StringComparison.OrdinalIgnoreCase) || raw.Equals("yes", StringComparison.OrdinalIgnoreCase) || raw.Equals("1", StringComparison.OrdinalIgnoreCase));

    private static bool? ParseNullableBool(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        if (raw.Equals("true", StringComparison.OrdinalIgnoreCase) || raw.Equals("yes", StringComparison.OrdinalIgnoreCase) || raw.Equals("1", StringComparison.OrdinalIgnoreCase))
            return true;
        if (raw.Equals("false", StringComparison.OrdinalIgnoreCase) || raw.Equals("no", StringComparison.OrdinalIgnoreCase) || raw.Equals("0", StringComparison.OrdinalIgnoreCase))
            return false;
        return null;
    }

    private static QuestionnaireTemplateImportRowDto MapRow(Dictionary<string, string?> dict, int rowNumber)
    {
        string? Get(string key) => dict.TryGetValue(key, out var v) ? v : null;
        string? GetTitle() => Get("QuestionTitle") ?? Get("PromptText"); // backward-compatible
        return new QuestionnaireTemplateImportRowDto
        {
            RowNumber = rowNumber,
            Scope = Get("Scope"),
            TemplateName = Get("TemplateName"),
            TemplateType = Get("TemplateType"),
            Title = Get("Title"),
            Description = Get("Description"),
            TargetSectionOrder = Get("TargetSectionOrder"),
            SectionOrder = Get("SectionOrder"),
            SectionTitle = Get("SectionTitle"),
            QuestionOrder = Get("QuestionOrder"),
            QuestionType = Get("QuestionType"),
            IsRequired = Get("IsRequired"),
            QuestionTitle = GetTitle(),
            TraitKey = Get("TraitKey"),
            Ws = Get("Ws"),
            OptionOrder = Get("OptionOrder"),
            OptionLabel = Get("OptionLabel"),
            IsCorrect = Get("IsCorrect"),
            Score = Get("Score"),
            Wa = Get("Wa")
        };
    }

    private static QuestionnaireTemplateImportValidationErrorDto Err(
        QuestionnaireTemplateImportRowDto row,
        string column,
        string message)
    {
        return new QuestionnaireTemplateImportValidationErrorDto
        {
            RowNumber = row.RowNumber,
            Column = column,
            Message = message,
            Severity = "Error"
        };
    }

    private static bool TryParseScope(string? raw, out QuestionnaireTemplateImportScope scope)
    {
        scope = default;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        return Enum.TryParse(raw.Trim(), true, out scope);
    }

    private static string? FirstNonEmpty(IEnumerable<string?> values)
        => values.Select(v => v?.Trim()).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    private static bool TryParsePositiveInt(string? raw, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var s = raw.Trim();

        // Fast path: plain integer
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return value > 0;

        // Google Sheets / Excel sometimes emits whole numbers as "1.0" in .xlsx numeric cells.
        // Accept decimals that represent an integer value.
        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
        {
            if (dec <= 0) return false;
            if (dec != decimal.Truncate(dec)) return false;
            if (dec > int.MaxValue) return false;
            value = (int)dec;
            return true;
        }

        return false;
    }

    private static bool TryParseDecimal(string? raw, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        return decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static bool IsOptionBasedType(string questionType)
    {
        return string.Equals(questionType, "SingleChoice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(questionType, "MultiChoice", StringComparison.OrdinalIgnoreCase)
               || string.Equals(questionType, "Likert", StringComparison.OrdinalIgnoreCase)
               || string.Equals(questionType, "Radio", StringComparison.OrdinalIgnoreCase)
               || string.Equals(questionType, "Checkbox", StringComparison.OrdinalIgnoreCase)
               || string.Equals(questionType, "Dropdown", StringComparison.OrdinalIgnoreCase);
    }
}
