using System.Text.RegularExpressions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Domain.Identity;

/// <summary>
/// Email Value Object. Normalizes to lowercase and validates basic format.
/// </summary>
public sealed partial class Email : ValueObject
{
    public string Value { get; }

    // Private ctor: a Email instance can ONLY be obtained through Create(),
    // which guarantees the invariants (format, length, normalization).
    // C# 14 primary constructors are public, so we keep the classic form here.
    private Email(string value) => Value = value;

    public static Email Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new InvalidEntityStateException("identity.email.required", "Email is required.");
        }

        var normalized = input.Trim().ToLowerInvariant();

        if (normalized.Length > 256)
        {
            throw new InvalidEntityStateException("identity.email.too_long", "Email exceeds 256 characters.");
        }

        if (!EmailRegex().IsMatch(normalized))
        {
            throw new InvalidEntityStateException("identity.email.invalid_format", "Email format is invalid.");
        }

        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
