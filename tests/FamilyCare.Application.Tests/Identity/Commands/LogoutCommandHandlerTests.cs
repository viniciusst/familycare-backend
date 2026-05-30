using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Commands.Logout;

namespace FamilyCare.Application.Tests.Identity.Commands;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IAuthTokenService> _tokenServiceMock = new();

    private LogoutCommandHandler CreateSut() => new(_tokenServiceMock.Object);

    [Fact]
    public async Task Handle_ShouldRevokeRefreshTokenWithLogoutReason()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.Handle(new LogoutCommand("refresh-to-revoke"), CancellationToken.None);

        // Assert
        _tokenServiceMock.Verify(
            s => s.RevokeAsync("refresh-to-revoke", "logout", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _validator = new();

    [Fact]
    public void Validate_WithNonEmptyToken_ShouldPass()
    {
        var result = _validator.Validate(new LogoutCommand("any-token"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyToken_ShouldFail()
    {
        var result = _validator.Validate(new LogoutCommand(""));

        Assert.False(result.IsValid);
    }
}
