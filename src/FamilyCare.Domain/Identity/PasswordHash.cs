using FamilyCare.Domain.Common;

namespace FamilyCare.Domain.Identity;

/// <summary>
/// Wraps an already-hashed password. The Domain never sees plaintext —
/// the Infrastructure layer is responsible for hashing and provides
/// the resulting string here.
/// </summary>
public sealed class PasswordHash : ValueObject
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static PasswordHash FromHashed(string hashedValue)
    {
        if (string.IsNullOrWhiteSpace(hashedValue))
        {
            throw new InvalidEntityStateException(
                "identity.password_hash.required",
                "Password hash cannot be empty.");
        }

        return new PasswordHash(hashedValue);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
