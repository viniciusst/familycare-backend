using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Commands.RefreshTokens;

namespace FamilyCare.Application.Tests.Identity.Commands;

public class RefreshTokensCommandHandlerTests
{
    private readonly Mock<IAuthTokenService> _tokenServiceMock = new();

    private RefreshTokensCommandHandler CreateSut() => new(_tokenServiceMock.Object);

    private static AuthTokens AnyTokens()
        => new("new-access", DateTime.UtcNow.AddMinutes(15),
               "new-refresh", DateTime.UtcNow.AddDays(30),
               UserId.New());

    [Fact]
    public async Task Handle_ShouldRotateRefreshTokenAndReturnNewPair()
    {
        // Arrange
        var newTokens = AnyTokens();
        _tokenServiceMock
            .Setup(s => s.RotateAsync("old-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTokens);

        var command = new RefreshTokensCommand("old-refresh");
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newTokens, result);
        _tokenServiceMock.Verify(
            s => s.RotateAsync("old-refresh", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServicePropagatesException_ShouldRethrow()
    {
        // Arrange — token service decides about validity; handler is a pass-through.
        _tokenServiceMock
            .Setup(s => s.RotateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("Invalid refresh token."));

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(new RefreshTokensCommand("bad"), CancellationToken.None));
    }
}

public class RefreshTokensCommandValidatorTests
{
    private readonly RefreshTokensCommandValidator _validator = new();

    [Fact]
    public void Validate_WithNonEmptyToken_ShouldPass()
    {
        var result = _validator.Validate(new RefreshTokensCommand("any-token"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    public void Validate_WithEmptyToken_ShouldFail(string token)
    {
        var result = _validator.Validate(new RefreshTokensCommand(token));

        Assert.False(result.IsValid);
    }
}
