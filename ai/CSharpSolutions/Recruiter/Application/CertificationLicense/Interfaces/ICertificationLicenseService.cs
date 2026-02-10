using Recruiter.Application.CertificationLicense.Dto;
using Ardalis.Result;

namespace Recruiter.Application.CertificationLicense.Interfaces;

public interface ICertificationLicenseService
{
    Task<IEnumerable<CertificationLicenseDto>> GetAllAsync();
    Task<CertificationLicenseDto?> GetByIdAsync(Guid id);
    Task<CertificationLicenseDto> CreateAsync(CertificationLicenseDto dto);
    Task<CertificationLicenseDto> UpdateAsync(CertificationLicenseDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Result<List<CertificationLicenseDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
