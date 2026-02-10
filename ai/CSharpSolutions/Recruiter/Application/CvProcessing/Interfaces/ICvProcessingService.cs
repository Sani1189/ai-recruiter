using Ardalis.Result;
using Recruiter.Application.CvProcessing.Dto;

namespace Recruiter.Application.CvProcessing.Interfaces;

public interface ICvProcessingService
{
    Task<Result<CvUploadResponseDto>> UploadCvAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid userProfileId,
        string promptCategory = "cv_extraction",
        string promptName = "CVExtractionScoringInstructions",
        int promptVersion = 1,
        CancellationToken cancellationToken = default);
}

