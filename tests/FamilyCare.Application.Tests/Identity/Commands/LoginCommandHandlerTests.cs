using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Commands.Login;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.Identity.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IAuthTokenService> _tokenServiceMock = new();

    private LoginCommandHandler CreateSut()
        => new(_userRepoMock.Object, _hasherMock.Object, _tokenServiceMock.Object);

    private static AuthTokens AnyTokens(UserId userId)
        => new(
            AccessToken: "access-jwt",
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15),
            RefreshToken: "refresh-opaque",
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(30),
            UserId: userId);

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldIssueTokens()
    {
        // Arrange
        var user = TestData.AnyUser(email: Email.Create("vinicius@familycare.test"));
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock
            .Setup(h => h.Verify("CorrectPass1!", user.PasswordHash))
            .Returns(true);
        _tokenServiceMock
            .Setup(s => s.IssueTokensAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AnyTokens(user.Id));

        var command = new LoginCommand("vinicius@familycare.test", "CorrectPass1!");
        var sut = CreateSut();

        // Act
        var tokens = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(user.Id, tokens.UserId);
        Assert.Equal("access-jwt", tokens.AccessToken);
        _tokenServiceMock.Verify(
            s => s.IssueTokensAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowForbiddenException()
    {
        // Arrange — same error for "user not found" and "wrong password" to avoid enumeration.
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("nobody@familycare.test", "AnyPass1!");
        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(command, CancellationToken.None));
        _tokenServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ShouldThrowForbiddenException()
    {
        // Arrange
        var user = TestData.AnyUser();
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<PasswordHash>()))
            .Returns(false);

        var command = new LoginCommand("anyone@familycare.test", "WrongPass1!");
        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(command, CancellationToken.None));
        _tokenServiceMock.VerifyNoOtherCalls();
    }
}

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new LoginCommand("user@test.com", "any-pass"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "pass")]
    [InlineData("bad-email", "pass")]
    [InlineData("user@test.com", "")]
    public void Validate_WithBadInput_ShouldFail(string email, string password)
    {
        var result = _validator.Validate(new LoginCommand(email, password));

        Assert.False(result.IsValid);
    }
}
