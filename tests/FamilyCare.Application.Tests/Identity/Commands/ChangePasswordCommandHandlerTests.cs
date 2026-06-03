using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Commands.ChangePassword;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.Identity.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();

    private ChangePasswordCommandHandler CreateSut() => new(
        _currentUserMock.Object, _userRepoMock.Object, _hasherMock.Object);

    [Fact]
    public async Task Handle_WithValidCurrentPassword_ShouldChangePassword()
    {
        // Arrange
        var user = TestData.AnyUser();
        var newHash = PasswordHash.FromHashed(
            "$2a$12$NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN");

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(user.Id);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("OldPass1!", user.PasswordHash)).Returns(true);
        _hasherMock.Setup(h => h.Hash("NewPass1!")).Returns(newHash);

        var sut = CreateSut();

        // Act
        await sut.Handle(new ChangePasswordCommand("OldPass1!", "NewPass1!"), CancellationToken.None);

        // Assert
        Assert.Equal(newHash, user.PasswordHash);
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFound()
    {
        // Arrange
        var userId = UserId.New();
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Handle(new ChangePasswordCommand("Old1!", "New1!"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIncorrect_ShouldThrowForbidden()
    {
        // Arrange
        var user = TestData.AnyUser();
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(user.Id);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<PasswordHash>()))
            .Returns(false);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(new ChangePasswordCommand("WrongOld1!", "NewPass1!"), CancellationToken.None));

        _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ShouldPropagateException()
    {
        // Arrange
        _currentUserMock
            .Setup(c => c.RequireUserId())
            .Throws<UnauthenticatedException>();

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<UnauthenticatedException>(
            () => sut.Handle(new ChangePasswordCommand("Old1!", "New1!"), CancellationToken.None));
    }
}

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidNewPassword_ShouldPass()
    {
        var result = _validator.Validate(new ChangePasswordCommand("OldPass1!", "NewPass1!"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNewEqualsCurrent_ShouldFail()
    {
        var result = _validator.Validate(new ChangePasswordCommand("SamePass1!", "SamePass1!"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoDigitsHere!")]
    public void Validate_WithWeakNewPassword_ShouldFail(string newPassword)
    {
        var result = _validator.Validate(new ChangePasswordCommand("OldPass1!", newPassword));

        Assert.False(result.IsValid);
    }
}
