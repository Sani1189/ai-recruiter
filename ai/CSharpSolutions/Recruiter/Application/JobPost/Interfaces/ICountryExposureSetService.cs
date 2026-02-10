namespace Recruiter.Application.JobPost.Interfaces;

/// <summary>
/// Resolves or creates a CountryExposureSet from a list of country codes.
/// Uses deterministic GUID from canonical string so the same set is reused across job posts.
/// </summary>
public interface ICountryExposureSetService
{
    /// <summary>
    /// Gets the Id of the exposure set for the given country codes, or creates it if it does not exist.
    /// Caller is responsible for SaveChanges after this returns.
    /// </summary>
    /// <param name="countryCodes">ISO-3166-1 alpha-2 codes (e.g. "NO", "IN", "US"). Empty or null returns null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The set Id, or null if no codes provided.</returns>
    Task<Guid?> GetOrCreateSetIdAsync(IReadOnlyList<string>? countryCodes, CancellationToken cancellationToken = default);
}
