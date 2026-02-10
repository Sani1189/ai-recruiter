using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Recruiter.Application.CvProcessing.Dto;
using Recruiter.Application.CvProcessing.Interfaces;

namespace Recruiter.Application.CvProcessing.Services;

public class CvProcessingService : ICvProcessingService
{
    private readonly HttpClient _httpClient;

    public CvProcessingService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Result<CvUploadResponseDto>> UploadCvAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid userProfileId,
        string promptCategory = "cv_extraction",
        string promptName = "CVExtractionScoringInstructions",
        int promptVersion = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"Uploading CV {fileName} for user {userProfileId}");

            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            
            content.Add(fileContent, "file", fileName);
            content.Add(new StringContent(promptCategory), "prompt_category");
            content.Add(new StringContent(promptName), "prompt_name");
            content.Add(new StringContent(promptVersion.ToString()), "prompt_version");
            content.Add(new StringContent(userProfileId.ToString()), "userProfileId");

            var response = await _httpClient.PostAsync("/api/upload-cv", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Python API error {response.StatusCode}: {errorContent}");
                return Result<CvUploadResponseDto>.Invalid(
                    new ValidationError { ErrorMessage = $"CV upload failed: {response.StatusCode} - {errorContent}" });
            }

            var result = await response.Content.ReadFromJsonAsync<CvUploadResponseDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);

            if (result == null)
            {
                Console.WriteLine("Failed to deserialize Python API response");
                return Result<CvUploadResponseDto>.Invalid(
                    new ValidationError { ErrorMessage = "Failed to process CV upload response" });
            }

            Console.WriteLine($"CV uploaded successfully. FileId: {result.FileId}, UserId: {result.UserId}");
            return Result<CvUploadResponseDto>.Success(result);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP error: {ex.Message}");
            return Result<CvUploadResponseDto>.Invalid(new ValidationError { ErrorMessage = $"Network error: {ex.Message}" });
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            Console.WriteLine($"Timeout: {ex.Message}");
            return Result<CvUploadResponseDto>.Invalid(new ValidationError { ErrorMessage = "Request timeout. Please try again." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Result<CvUploadResponseDto>.Invalid(new ValidationError { ErrorMessage = $"An error occurred: {ex.Message}" });
        }
    }
}

