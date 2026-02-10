using System;
using System.Collections.Generic;
using System.Text;

namespace Recruiter.Application.Common.Helpers;

public class DeterministicGuidHelper
{
    /// <summary>Generates a deterministic GUID from a canonical string (e.g. sorted country codes).</summary>
    public static Guid CreateDeterministicGuid(string canonical)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonical));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }
}
