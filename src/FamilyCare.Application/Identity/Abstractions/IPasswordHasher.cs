using FamilyCare.Domain.Identity;

namespace FamilyCare.Application.Identity.Abstractions;

/// <summary>
/// Abstraction over password hashing. Domain never sees the plaintext —
/// the application receives plaintext, hashes it via this service, and
/// stores the resulting PasswordHash VO on the aggregate.
/// </summary>
public interface IPasswordHasher
{
    PasswordHash Hash(string plaintextPassword);

    bool Verify(string plaintextPassword, PasswordHash hash);
}
