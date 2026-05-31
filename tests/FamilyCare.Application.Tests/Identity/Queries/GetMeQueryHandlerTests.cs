using FamilyCare.Application.Identity.Queries.GetMe;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.Identity.Queries;

public class GetMeQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();

    private GetMeQueryHandler CreateSut() => new(_currentUserMock.Object, _userRepoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnUserProfile()
    {
        // Arrange
        var email = Email.Create("vinicius@familycare.test");
        var user = TestData.AnyUser(email: email, language: SupportedLanguage.PortugueseBrazil);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(user.Id);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var sut = CreateSut();

        // Act
        var dto = await sut.Handle(new GetMeQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal("vinicius@familycare.test", dto.Email);
        Assert.Equal(SupportedLanguage.PortugueseBrazil, dto.PreferredLanguage);
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
            () => sut.Handle(new GetMeQuery(), CancellationToken.None));
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
            () => sut.Handle(new GetMeQuery(), CancellationToken.None));

        _userRepoMock.Verify(
            r => r.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
