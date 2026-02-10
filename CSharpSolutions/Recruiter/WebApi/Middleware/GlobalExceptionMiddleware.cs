using Microsoft.EntityFrameworkCore;
using Recruiter.Application.Common.Dto;
using System.Net;
using System.Text.Json;

namespace Recruiter.WebApi.Middleware;

/// Global exception handling middleware
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (errorResponse, statusCode) = exception switch
        {
            ArgumentNullException nullEx => 
                (CreateErrorResponse(
                    "Required parameter is missing",
                    "MISSING_PARAMETER",
                    HttpStatusCode.BadRequest,
                    nullEx.ParamName), HttpStatusCode.BadRequest),
            
            ArgumentException argEx => 
                (CreateErrorResponse(
                    argEx.Message,
                    "INVALID_ARGUMENT",
                    HttpStatusCode.BadRequest), HttpStatusCode.BadRequest),

            InvalidOperationException opEx when opEx.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) =>
                (CreateErrorResponse(
                    opEx.Message,
                    "DUPLICATE_ENTRY",
                    HttpStatusCode.Conflict), HttpStatusCode.Conflict),

            InvalidOperationException opEx when opEx.Message.Contains("job post", StringComparison.OrdinalIgnoreCase) && opEx.Message.Contains("existing applications", StringComparison.OrdinalIgnoreCase) =>
                (CreateErrorResponse(
                    opEx.Message,
                    "JOB_POST_HAS_APPLICATIONS",
                    HttpStatusCode.BadRequest), HttpStatusCode.BadRequest),

            InvalidOperationException opEx when opEx.Message.Contains("step", StringComparison.OrdinalIgnoreCase) && opEx.Message.Contains("existing applications", StringComparison.OrdinalIgnoreCase) =>
                (CreateErrorResponse(
                    opEx.Message,
                    "JOB_STEP_HAS_APPLICATIONS",
                    HttpStatusCode.BadRequest), HttpStatusCode.BadRequest),

            InvalidOperationException opEx =>
                (CreateErrorResponse(
                    opEx.Message,
                    "INVALID_OPERATION",
                    HttpStatusCode.BadRequest), HttpStatusCode.BadRequest),
            
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => 
                (CreateErrorResponse(
                    "A record with this information already exists",
                    "DUPLICATE_ENTRY",
                    HttpStatusCode.Conflict,
                    details: dbEx.InnerException?.Message,
                    field: GetFieldFromUniqueConstraint(dbEx)), HttpStatusCode.Conflict),

            DbUpdateConcurrencyException concurrencyEx =>
                (CreateErrorResponse(
                    "The record was modified by another process. Please refresh and try again.",
                    "CONCURRENCY_CONFLICT",
                    HttpStatusCode.Conflict,
                    concurrencyEx.Message), HttpStatusCode.Conflict),
            
            DbUpdateException dbEx => 
                (CreateErrorResponse(
                    "Database operation failed",
                    "DATABASE_ERROR",
                    HttpStatusCode.InternalServerError,
                    dbEx.InnerException?.Message), HttpStatusCode.InternalServerError),
            
            UnauthorizedAccessException => 
                (CreateErrorResponse(
                    "Access denied",
                    "UNAUTHORIZED",
                    HttpStatusCode.Unauthorized), HttpStatusCode.Unauthorized),
            
            KeyNotFoundException => 
                (CreateErrorResponse(
                    "Resource not found",
                    "NOT_FOUND",
                    HttpStatusCode.NotFound), HttpStatusCode.NotFound),
            
            _ => (CreateErrorResponse(
                "An unexpected error occurred",
                "INTERNAL_ERROR",
                HttpStatusCode.InternalServerError,
                exception.Message), HttpStatusCode.InternalServerError)
        };

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static ErrorResponseDto CreateErrorResponse(
        string message, 
        string errorCode, 
        HttpStatusCode statusCode, 
        string? details = null, 
        string? field = null)
    {
        return new ErrorResponseDto
        {
            Message = message,
            Details = details,
            Field = field,
            ErrorCode = errorCode,
            Timestamp = DateTime.UtcNow
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("UNIQUE constraint") == true ||
               ex.InnerException?.Message.Contains("duplicate key") == true ||
               ex.InnerException?.Message.Contains("Cannot insert duplicate key") == true;
    }

    private static string? GetFieldFromUniqueConstraint(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? string.Empty;
        
        // QuestionnaireTemplate / QuestionnaireSection / QuestionnaireQuestion / QuestionnaireQuestionOption
        // These help pinpoint which unique constraint was violated.
        if (message.Contains("PK_QuestionnaireTemplates"))
            return "template.name";
        if (message.Contains("PK_QuestionnaireQuestions"))
            return "sections.questions.name";
        if (message.Contains("PK_QuestionnaireQuestionOptions"))
            return "sections.questions.options.name";

        // Unique order constraints
        if (message.Contains("IX_QuestionnaireSections_QuestionnaireTemplateName_QuestionnaireTemplateVersion_Order"))
            return "sections.order";
        if (message.Contains("IX_QuestionnaireQuestions_QuestionnaireSectionId_Order"))
            return "sections.questions.order";
        if (message.Contains("IX_QuestionnaireQuestionOptions_QuestionnaireQuestionName_QuestionnaireQuestionVersion_Order"))
            return "sections.questions.options.order";
            
        return null;
    }
}

/// Extension method to register the middleware
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
