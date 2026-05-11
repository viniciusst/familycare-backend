using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Domain.Identity;

namespace FamilyCare.Infrastructure.Identity.Services;

/// <summary>
/// BCrypt-based password hasher. Work factor 12 is a reasonable default in 2026
/// (about ~250ms per hash on commodity hardware).
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public PasswordHash Hash(string plaintextPassword)
    {
        if (string.IsNullOrWhiteSpace(plaintextPassword))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(plaintextPassword));
        }

        var hashed = BCrypt.Net.BCrypt.HashPassword(plaintextPassword, WorkFactor);
        return PasswordHash.FromHashed(hashed);
    }

    public bool Verify(string plaintextPassword, PasswordHash hash)
    {
        if (string.IsNullOrWhiteSpace(plaintextPassword))
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(hash);

        try
        {
            return BCrypt.Net.BCrypt.Verify(plaintextPassword, hash.Value);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash is malformed or from a different algorithm — treat as failed verification.
            return false;
        }
    }
}
