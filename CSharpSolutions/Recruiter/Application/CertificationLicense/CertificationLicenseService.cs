using AutoMapper;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Application.CertificationLicense.Dto;
using Recruiter.Application.CertificationLicense.Interfaces;
using Recruiter.Application.CertificationLicense.Specifications;
using Ardalis.Result;

namespace Recruiter.Application.CertificationLicense;

public class CertificationLicenseService : ICertificationLicenseService
{
    private readonly IRepository<Domain.Models.CertificationLicense> _repository;
    private readonly IMapper _mapper;

    public CertificationLicenseService(IRepository<Domain.Models.CertificationLicense> repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<CertificationLicenseDto>> GetAllAsync()
    {
        var entities = await _repository.ListAsync();
        return _mapper.Map<IEnumerable<CertificationLicenseDto>>(entities);
    }

    public async Task<CertificationLicenseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? _mapper.Map<CertificationLicenseDto>(entity) : null;
    }

    public async Task<CertificationLicenseDto> CreateAsync(CertificationLicenseDto dto)
    {
        var entity = _mapper.Map<Domain.Models.CertificationLicense>(dto);
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<CertificationLicenseDto>(entity);
    }

    public async Task<CertificationLicenseDto> UpdateAsync(CertificationLicenseDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id!.Value);
        if (entity == null)
        {
            throw new InvalidOperationException($"CertificationLicense with id '{dto.Id}' not found.");
        }

        _mapper.Map(dto, entity);

        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<CertificationLicenseDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity != null)
        {
            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null;
    }

    public async Task<Result<List<CertificationLicenseDto>>> GetByUserProfileIdAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var spec = new CertificationLicenseByUserProfileSpec(userProfileId);
        var entities = await _repository.ListAsync(spec, cancellationToken);
        var dtos = _mapper.Map<List<CertificationLicenseDto>>(entities);
        return Result<List<CertificationLicenseDto>>.Success(dtos);
    }
}
