using FamilyCare.Domain.Common;
using FamilyCare.Domain.Identity.Events;

namespace FamilyCare.Domain.Identity;

/// <summary>
/// User aggregate. Carries authentication credentials and language preference.
/// A User can belong to many Families (modeled via FamilyMember).
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public SupportedLanguage PreferredLanguage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Private ctor for EF Core / serialization
    private User() : base()
    {
        Email = null!;
        PasswordHash = null!;
    }

    private User(
        UserId id,
        Email email,
        PasswordHash passwordHash,
        SupportedLanguage preferredLanguage,
        DateTime createdAt) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        PreferredLanguage = preferredLanguage;
        CreatedAt = createdAt;
    }

    /// <summary>Factory method: creates and registers a new user.</summary>
    public static User Register(
        Email email,
        PasswordHash passwordHash,
        SupportedLanguage preferredLanguage)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(passwordHash);

        var user = new User(
            UserId.New(),
            email = Email.Create(email.Value),
            passwordHash,
            preferredLanguage,
            DateTime.UtcNow);

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, email.Value, DateTime.UtcNow));
        return user;
    }

    public void ChangePassword(PasswordHash newHash)
    {
        ArgumentNullException.ThrowIfNull(newHash);

        if (this.PasswordHash.Equals(newHash))
        {
            return;
        }

        PasswordHash = newHash;
        RaiseDomainEvent(new UserPasswordChangedEvent(Id, DateTime.UtcNow));
    }

    public void ChangePreferredLanguage(SupportedLanguage language)
    {
        if (PreferredLanguage == language)
        {
            return;
        }

        PreferredLanguage = language;
        RaiseDomainEvent(new UserPreferredLanguageChangedEvent(Id, language, DateTime.UtcNow));
    }
}
