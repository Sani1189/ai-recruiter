using AutoMapper;
using Recruiter.Application.CertificationLicense.Dto;

namespace Recruiter.Application.CertificationLicense;

public class CertificationLicenseProfile : Profile
{
    public CertificationLicenseProfile()
    {
        CreateMap<Domain.Models.CertificationLicense, CertificationLicenseDto>();
        CreateMap<CertificationLicenseDto, Domain.Models.CertificationLicense>();
    }
}
