using FluentAssertions;
using Recruiter.Application.Questionnaire.Dto;
using Recruiter.Domain.Enums;
using Recruiter.Domain.Models;

namespace Recruiter.UnitTests;

public class QuestionnaireScoringTests
{
    [Fact]
    public void Quiz_MultiChoice_IgnoresIncorrectOptionScores_TotalDoesNotExceedMax()
    {
        var question = new QuestionnaireQuestion
        {
            Name = "q1",
            Version = 1,
            Order = 1,
            QuestionType = QuestionnaireQuestionTypeEnum.MultiChoice,
            Options = new List<QuestionnaireQuestionOption>
            {
                new()
                {
                    Name = "o1",
                    Version = 1,
                    Order = 1,
                    IsCorrect = true,
                    Score = 10m
                },
                new()
                {
                    Name = "o2",
                    Version = 1,
                    Order = 2,
                    IsCorrect = false,
                    Score = 5.5m // misconfigured positive score on incorrect option
                }
            }
        };

        var questionByKey = new Dictionary<(string Name, int Version), QuestionnaireQuestion>
        {
            [(question.Name, question.Version)] = question
        };

        var incoming = new List<CandidateQuestionnaireAnswerDto>
        {
            new()
            {
                QuestionName = "q1",
                QuestionVersion = 1,
                SelectedOptions = new List<QuestionOptionReferenceDto>
                {
                    new() { OptionName = "o1", OptionVersion = 1 },
                    new() { OptionName = "o2", OptionVersion = 1 }
                }
            }
        };

        var result = InvokeBuildAnswers(incoming, questionByKey, QuestionnaireTemplateTypeEnum.Quiz);

        GetDecimal(result, "TotalScore").Should().Be(10m);
        GetDecimal(result, "MaxScore").Should().Be(10m);

        var answers = GetList(result, "Answers");
        answers.Should().HaveCount(1);
        GetNullableDecimal(answers[0], "ScoreAwarded").Should().Be(10m);
    }

    [Fact]
    public void Quiz_SingleChoice_IncorrectSelectionDoesNotAwardScore()
    {
        var question = new QuestionnaireQuestion
        {
            Name = "q1",
            Version = 1,
            Order = 1,
            QuestionType = QuestionnaireQuestionTypeEnum.SingleChoice,
            Options = new List<QuestionnaireQuestionOption>
            {
                new()
                {
                    Name = "correct",
                    Version = 1,
                    Order = 1,
                    IsCorrect = true,
                    Score = 10m
                },
                new()
                {
                    Name = "incorrect",
                    Version = 1,
                    Order = 2,
                    IsCorrect = false,
                    Score = 20m // misconfigured higher score on incorrect option
                }
            }
        };

        var questionByKey = new Dictionary<(string Name, int Version), QuestionnaireQuestion>
        {
            [(question.Name, question.Version)] = question
        };

        var incoming = new List<CandidateQuestionnaireAnswerDto>
        {
            new()
            {
                QuestionName = "q1",
                QuestionVersion = 1,
                SelectedOptions = new List<QuestionOptionReferenceDto>
                {
                    new() { OptionName = "incorrect", OptionVersion = 1 }
                }
            }
        };

        var result = InvokeBuildAnswers(incoming, questionByKey, QuestionnaireTemplateTypeEnum.Quiz);

        GetDecimal(result, "TotalScore").Should().Be(0m);
        GetDecimal(result, "MaxScore").Should().Be(10m); // quiz uses correct options when correctness is configured

        var answers = GetList(result, "Answers");
        answers.Should().HaveCount(1);
        GetNullableDecimal(answers[0], "ScoreAwarded").Should().Be(0m);
    }

    [Fact]
    public void Quiz_WhenIsCorrectIsNullEverywhere_FallsBackToScoreBasedScoring()
    {
        var question = new QuestionnaireQuestion
        {
            Name = "q1",
            Version = 1,
            Order = 1,
            QuestionType = QuestionnaireQuestionTypeEnum.MultiChoice,
            Options = new List<QuestionnaireQuestionOption>
            {
                new()
                {
                    Name = "o1",
                    Version = 1,
                    Order = 1,
                    IsCorrect = null,
                    Score = 10m
                },
                new()
                {
                    Name = "o2",
                    Version = 1,
                    Order = 2,
                    IsCorrect = null,
                    Score = 0m
                }
            }
        };

        var questionByKey = new Dictionary<(string Name, int Version), QuestionnaireQuestion>
        {
            [(question.Name, question.Version)] = question
        };

        var incoming = new List<CandidateQuestionnaireAnswerDto>
        {
            new()
            {
                QuestionName = "q1",
                QuestionVersion = 1,
                SelectedOptions = new List<QuestionOptionReferenceDto>
                {
                    new() { OptionName = "o1", OptionVersion = 1 }
                }
            }
        };

        var result = InvokeBuildAnswers(incoming, questionByKey, QuestionnaireTemplateTypeEnum.Quiz);

        GetDecimal(result, "TotalScore").Should().Be(10m);
        GetDecimal(result, "MaxScore").Should().Be(10m);

        var answers = GetList(result, "Answers");
        answers.Should().HaveCount(1);
        GetNullableDecimal(answers[0], "ScoreAwarded").Should().Be(10m);
    }

    [Fact]
    public void Validator_Likert_DoesNotAllowMultipleSelections()
    {
        var question = new QuestionnaireQuestion
        {
            Name = "likert",
            Version = 1,
            Order = 1,
            QuestionType = QuestionnaireQuestionTypeEnum.Likert,
            IsRequired = true,
            Options = new List<QuestionnaireQuestionOption>
            {
                new() { Name = "o1", Version = 1, Order = 1, Score = 1m },
                new() { Name = "o2", Version = 1, Order = 2, Score = 2m }
            }
        };

        var questionByKey = new Dictionary<(string Name, int Version), QuestionnaireQuestion>
        {
            [(question.Name, question.Version)] = question
        };

        var request = new CandidateQuestionnaireSubmitRequestDto
        {
            Answers = new List<CandidateQuestionnaireAnswerDto>
            {
                new()
                {
                    QuestionName = "likert",
                    QuestionVersion = 1,
                    SelectedOptions = new List<QuestionOptionReferenceDto>
                    {
                        new() { OptionName = "o1", OptionVersion = 1 },
                        new() { OptionName = "o2", OptionVersion = 1 }
                    }
                }
            }
        };

        var errors = InvokeValidateRequest(request, questionByKey);
        errors.Should().Contain(e => e.ErrorMessage.Contains("Only one option", StringComparison.OrdinalIgnoreCase));
    }

    private static List<Ardalis.Result.ValidationError> InvokeValidateRequest(
        CandidateQuestionnaireSubmitRequestDto request,
        Dictionary<(string Name, int Version), QuestionnaireQuestion> questionByKey)
    {
        var t = typeof(CandidateQuestionnaireSubmitRequestDto).Assembly
            .GetType("Recruiter.Application.Questionnaire.QuestionnaireSubmissionValidator", throwOnError: true)!;
        var mi = t.GetMethod("ValidateRequest", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        mi.Should().NotBeNull();

        return (List<Ardalis.Result.ValidationError>)mi!.Invoke(null, new object?[] { request, questionByKey })!;
    }

    private static object InvokeBuildAnswers(
        List<CandidateQuestionnaireAnswerDto> incoming,
        Dictionary<(string Name, int Version), QuestionnaireQuestion> questionByKey,
        QuestionnaireTemplateTypeEnum templateType)
    {
        // QuestionnaireAnswerBuilder is internal to Application; call via reflection.
        var t = typeof(CandidateQuestionnaireSubmitRequestDto).Assembly
            .GetType("Recruiter.Application.Questionnaire.QuestionnaireAnswerBuilder", throwOnError: true)!;
        var mi = t.GetMethod("BuildAnswers", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        mi.Should().NotBeNull();

        return mi!.Invoke(null, new object?[] { incoming, questionByKey, templateType, DateTimeOffset.UtcNow })!;
    }

    private static decimal GetDecimal(object obj, string propertyName)
    {
        var p = obj.GetType().GetProperty(propertyName);
        p.Should().NotBeNull();
        return (decimal)p!.GetValue(obj)!;
    }

    private static decimal? GetNullableDecimal(object obj, string propertyName)
    {
        var p = obj.GetType().GetProperty(propertyName);
        p.Should().NotBeNull();
        return (decimal?)p!.GetValue(obj);
    }

    private static List<object> GetList(object obj, string propertyName)
    {
        var p = obj.GetType().GetProperty(propertyName);
        p.Should().NotBeNull();

        var enumerable = (System.Collections.IEnumerable)p!.GetValue(obj)!;
        var list = new List<object>();
        foreach (var item in enumerable)
            list.Add(item!);
        return list;
    }
}
public class QuestionnaireImportXlsxReaderTests
{
    [Fact]
    public void SpreadsheetReader_Xlsx_ParsesImportWorksheetRowsByHeader()
    {
        using var stream = BuildMinimalXlsx();

        var t = typeof(CandidateQuestionnaireSubmitRequestDto).Assembly
            .GetType("Recruiter.Application.QuestionnaireTemplate.Import.SpreadsheetMl.SpreadsheetMlReader", throwOnError: true)!;
        var mi = t.GetMethod("ReadWorksheetRowsByHeader", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        mi.Should().NotBeNull();

        var rows = (IReadOnlyList<Dictionary<string, string?>>)mi!.Invoke(null, new object?[] { stream, "Import" })!;

        rows.Should().HaveCount(1);
        rows[0].Should().ContainKey("Scope");
        rows[0]["Scope"].Should().Be("CreateTemplate");
        rows[0]["TemplateName"].Should().Be("basic-math-quiz");
        rows[0]["QuestionTitle"].Should().Be("What is 2+2?");
        rows[0]["OptionLabel"].Should().Be("4");
    }

    private static MemoryStream BuildMinimalXlsx()
    {
        // Minimal OpenXML package with:
        // - workbook.xml + workbook.xml.rels mapping sheet name "Import" -> worksheets/sheet1.xml
        // - sharedStrings.xml for string values
        // - worksheet sheet1.xml with header row, hint row, and one data row.
        var ms = new MemoryStream();
        using (var zip = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zip, "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Import" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);

            AddEntry(zip, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1"
                                Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"
                                Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);

            // Shared strings: headers + values
            // 0 Scope, 1 TemplateName, 2 QuestionTitle, 3 OptionLabel, 4 CreateTemplate, 5 basic-math-quiz, 6 What is 2+2?, 7 4
            AddEntry(zip, "xl/sharedStrings.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="8" uniqueCount="8">
                  <si><t>Scope</t></si>
                  <si><t>TemplateName</t></si>
                  <si><t>QuestionTitle</t></si>
                  <si><t>OptionLabel</t></si>
                  <si><t>CreateTemplate</t></si>
                  <si><t>basic-math-quiz</t></si>
                  <si><t>What is 2+2?</t></si>
                  <si><t>4</t></si>
                </sst>
                """);

            AddEntry(zip, "xl/worksheets/sheet1.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData>
                    <row r="1">
                      <c r="A1" t="s"><v>0</v></c>
                      <c r="B1" t="s"><v>1</v></c>
                      <c r="C1" t="s"><v>2</v></c>
                      <c r="D1" t="s"><v>3</v></c>
                    </row>
                    <row r="2">
                      <c r="A2" t="s"><v>0</v></c>
                    </row>
                    <row r="3">
                      <c r="A3" t="s"><v>4</v></c>
                      <c r="B3" t="s"><v>5</v></c>
                      <c r="C3" t="s"><v>6</v></c>
                      <c r="D3" t="s"><v>7</v></c>
                    </row>
                  </sheetData>
                </worksheet>
                """);
        }

        ms.Position = 0;
        return ms;

        static void AddEntry(System.IO.Compression.ZipArchive zip, string path, string content)
        {
            var e = zip.CreateEntry(path);
            using var s = e.Open();
            using var w = new StreamWriter(s);
            w.Write(content);
        }
    }
}

public class QuestionnaireImportRealTemplateXlsxTests
{
    [Fact]
    public void DocsTemplate_Xlsx_ReadsHeadersAndSectionOrder()
    {
        var filePath = FindFileUpwards(AppContext.BaseDirectory, Path.Combine("docs", "questionnaire-import-template.xlsx"));
        filePath.Should().NotBeNull("Expected questionnaire import template under repo /docs");
        System.IO.File.Exists(filePath!).Should().BeTrue($"Expected template at {filePath}");

        using var fs = System.IO.File.OpenRead(filePath!);

        var t = typeof(CandidateQuestionnaireSubmitRequestDto).Assembly
            .GetType("Recruiter.Application.QuestionnaireTemplate.Import.SpreadsheetMl.SpreadsheetMlReader", throwOnError: true)!;
        var mi = t.GetMethod("ReadWorksheetRowsByHeader", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        mi.Should().NotBeNull();

        var rows = (IReadOnlyList<Dictionary<string, string?>>)mi!.Invoke(null, new object?[] { fs, "Import" })!;
        rows.Should().NotBeNull();
        rows.Count.Should().BeGreaterThan(0);

        // Just ensure the headers normalize to our expected keys and at least one row has SectionOrder set.
        rows[0].Keys.Should().Contain("SectionOrder");
        rows.Any(r => r.TryGetValue("SectionOrder", out var v) && !string.IsNullOrWhiteSpace(v)).Should().BeTrue();
    }

    private static string? FindFileUpwards(string startDir, string relativePath, int maxLevels = 12)
    {
        var dir = new DirectoryInfo(startDir);
        for (var i = 0; i < maxLevels && dir != null; i++)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (System.IO.File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        return null;
    }
}
