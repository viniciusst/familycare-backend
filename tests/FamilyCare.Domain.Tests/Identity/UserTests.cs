using FamilyCare.Domain.Tests.Common;

namespace FamilyCare.Domain.Tests.Identity;

public class UserTests
{
    [Fact]
    public void Register_ShouldCreateUserInPendingState()
    {
        var email = TestData.AnyEmail();
        var hash = TestData.AnyPasswordHash();

        var user = User.Register(email, hash, SupportedLanguage.PortugueseBrazil);

        Assert.NotEqual(UserId.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(hash, user.PasswordHash);
        Assert.Equal(SupportedLanguage.PortugueseBrazil, user.PreferredLanguage);
        Assert.InRange(user.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public void Register_ShouldRaiseUserRegisteredEvent()
    {
        var user = TestData.AnyUser();

        Assert.Single(user.DomainEvents, e => e is UserRegisteredEvent);
    }

    [Fact]
    public void ChangePassword_ShouldUpdateHash()
    {
        var user = TestData.AnyUser();
        var newHash = PasswordHash.FromHashed(
            "$2a$12$ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ");

        user.ChangePassword(newHash);

        Assert.Equal(newHash, user.PasswordHash);
    }

    [Fact]
    public void ChangePreferredLanguage_ShouldUpdateLanguage()
    {
        var user = TestData.AnyUser(language: SupportedLanguage.EnglishCanada);

        user.ChangePreferredLanguage(SupportedLanguage.FrenchCanada);

        Assert.Equal(SupportedLanguage.FrenchCanada, user.PreferredLanguage);
    }
}
