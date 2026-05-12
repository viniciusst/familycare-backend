namespace FamilyCare.Domain.Tests.Identity;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user+tag@example.com")]
    [InlineData("user@sub.example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    public void Create_WithValidEmail_ShouldSucceed(string input)
    {
        var email = Email.Create(input);

        Assert.False(string.IsNullOrEmpty(email.Value));
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        var email = Email.Create("USER@EXAMPLE.COM");

        Assert.Equal("user@example.com", email.Value);
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var email = Email.Create("  user@example.com  ");

        Assert.Equal("user@example.com", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyValue_ShouldThrow(string? input)
    {
        Assert.Throws<InvalidEntityStateException>(() => Email.Create(input!));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    public void Create_WithInvalidFormat_ShouldThrow(string input)
    {
        Assert.Throws<InvalidEntityStateException>(() => Email.Create(input));
    }

    [Fact]
    public void TwoEmailsWithSameValue_ShouldBeEqual()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("user@example.com");

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void TwoEmailsWithDifferentValues_ShouldNotBeEqual()
    {
        var a = Email.Create("first@example.com");
        var b = Email.Create("second@example.com");

        Assert.NotEqual(a, b);
    }
}
