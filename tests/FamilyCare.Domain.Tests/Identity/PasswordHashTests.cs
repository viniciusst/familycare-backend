namespace FamilyCare.Domain.Tests.Identity;

public class PasswordHashTests
{
    private const string ValidBcryptHash =
        "$2a$12$abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQR";

    [Fact]
    public void FromHashed_WithValidHash_ShouldSucceed()
    {
        var hash = PasswordHash.FromHashed(ValidBcryptHash);

        Assert.Equal(ValidBcryptHash, hash.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FromHashed_WithEmptyValue_ShouldThrow(string? input)
    {
        Assert.Throws<InvalidEntityStateException>(() => PasswordHash.FromHashed(input!));
    }

    [Fact]
    public void TwoHashesWithSameValue_ShouldBeEqual()
    {
        var a = PasswordHash.FromHashed(ValidBcryptHash);
        var b = PasswordHash.FromHashed(ValidBcryptHash);

        Assert.Equal(a, b);
        Assert.True(a == b);
    }
}
