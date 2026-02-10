using Recruiter.Application.Common.Helpers;
using Recruiter.Application.JobPost.Interfaces;
using Recruiter.Application.JobPost.Specifications;
using Recruiter.Application.Common.Interfaces;
using Recruiter.Domain.Models;

namespace Recruiter.Application.JobPost.Services;

/// <summary>
/// Resolves or creates CountryExposureSet from country codes using deterministic GUID.
/// Does not call SaveChanges; caller (e.g. JobPostService) must save after assigning the set to an entity.
/// </summary>
public class CountryExposureSetService : ICountryExposureSetService
{
    private readonly IRepository<CountryExposureSet> _setRepository;
    private readonly IRepository<CountryExposureSetCountry> _junctionRepository;

    public CountryExposureSetService(
        IRepository<CountryExposureSet> setRepository,
        IRepository<CountryExposureSetCountry> junctionRepository)
    {
        _setRepository = setRepository;
        _junctionRepository = junctionRepository;
    }

    /// <inheritdoc />
    public async Task<Guid?> GetOrCreateSetIdAsync(IReadOnlyList<string>? countryCodes, CancellationToken cancellationToken = default)
    {
        var canonical = BuildCanonical(countryCodes);
        if (string.IsNullOrEmpty(canonical))
            return null;

        var id = DeterministicGuidHelper.CreateDeterministicGuid(canonical);
        var spec = new CountryExposureSetByIdSpec(id);
        var existing = await _setRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (existing != null)
            return existing.Id;

        var set = new CountryExposureSet
        {
            Id = id,
            Canonical = canonical,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await _setRepository.AddAsync(set, cancellationToken);

        var codes = GetNormalizedCodes(countryCodes);
        foreach (var code in codes)
        {
            var junction = new CountryExposureSetCountry
            {
                Id = Guid.NewGuid(),
                CountryExposureSetId = id,
                CountryCode = code,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await _junctionRepository.AddAsync(junction, cancellationToken);
        }

        return id;
    }

    /// <summary>
    /// Builds a deterministic canonical string: sorted, comma-separated country codes (e.g. "IN,NO,US").
    /// </summary>
    internal static string BuildCanonical(IReadOnlyList<string>? countryCodes)
    {
        var codes = GetNormalizedCodes(countryCodes);
        if (codes.Count == 0)
            return string.Empty;
        return string.Join(",", codes.OrderBy(c => c, StringComparer.Ordinal));
    }

    private static List<string> GetNormalizedCodes(IReadOnlyList<string>? countryCodes)
    {
        if (countryCodes == null || countryCodes.Count == 0)
            return new List<string>();
        return countryCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct()
            .ToList();
    }
}
