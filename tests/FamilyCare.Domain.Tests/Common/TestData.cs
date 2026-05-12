namespace FamilyCare.Domain.Tests.Common;

internal static class TestData
{
    public static Email AnyEmail(string? local = null)
        => Email.Create($"{local ?? Guid.NewGuid().ToString("N")[..8]}@familycare.test");

    public static PasswordHash AnyPasswordHash()
        => PasswordHash.FromHashed("$2a$12$abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQR");

    public static User AnyUser(
        string? email = null,
        SupportedLanguage language = SupportedLanguage.EnglishCanada)
        => User.Register(
            email: Email.Create(email ?? $"{Guid.NewGuid():N}@familycare.test"),
            passwordHash: AnyPasswordHash(),
            preferredLanguage: language);

    public static Family AnyFamily(
        UserId? ownerUserId = null,
        string name = "Test Family",
        string ownerDisplayName = "Owner",
        DateOnly? ownerBirthDate = null)
        => Family.Create(
            name: name,
            ownerUserId: ownerUserId ?? UserId.New(),
            ownerDisplayName: ownerDisplayName,
            ownerBirthDate: ownerBirthDate ?? new DateOnly(1985, 1, 15));
}
