namespace FamilyCare.Application.Tests.Common;

internal static class TestData
{
    public static Email AnyEmail(string? local = null)
        => Email.Create($"{local ?? Guid.NewGuid().ToString("N")[..8]}@familycare.test");

    public static PasswordHash AnyPasswordHash()
        => PasswordHash.FromHashed("$2a$12$abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQR");

    public static User AnyUser(
        Email? email = null,
        SupportedLanguage language = SupportedLanguage.EnglishCanada)
        => User.Register(
            email: email ?? AnyEmail(),
            passwordHash: AnyPasswordHash(),
            preferredLanguage: language);
}
