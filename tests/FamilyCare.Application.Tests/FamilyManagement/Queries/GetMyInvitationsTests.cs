using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Queries.GetMyInvitations;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Queries;

public class GetMyInvitationsQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private GetMyInvitationsQueryHandler CreateSut()
        => new(_currentUserMock.Object, _userRepoMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_ShouldReturnInvitationsAddressedToUserEmail()
    {
        // Arrange
        var userId = UserId.New();
        var userEmail = Email.Create("user@example.com");
        var user = TestData.AnyUser(email: userEmail);
        var family = TestData.AnyFamily(name: "Smith Family");
        var invitation = TestData.AnyInvitation(family: family, email: userEmail);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _familyRepoMock
            .Setup(r => r.GetInvitationsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<InvitationStatus?>(),
                It.IsAny<PagedRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<(Family, Invitation)>(
                new[] { (family, invitation) }, Page: 1, PageSize: 20, TotalCount: 1));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetMyInvitationsQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("user@example.com", result.Items[0].Email);
        Assert.Equal("Smith Family", result.Items[0].FamilyName);
        Assert.Equal(family.Id, result.Items[0].FamilyId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WhenStatusFilterProvided_ShouldForwardToRepository()
    {
        // Arrange
        var userId = UserId.New();
        var userEmail = Email.Create("user@example.com");
        var user = TestData.AnyUser(email: userEmail);
        var family = TestData.AnyFamily(name: "Smith Family");
        var invitation = TestData.AnyInvitation(family: family, email: userEmail);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _familyRepoMock
            .Setup(r => r.GetInvitationsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<InvitationStatus?>(),
                It.IsAny<PagedRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<(Family, Invitation)>(
                new[] { (family, invitation) }, 1, 20, 1));

        var sut = CreateSut();

        // Act
        await sut.Handle(
            new GetMyInvitationsQuery(Status: InvitationStatus.Pending),
            CancellationToken.None);

        // Assert — repository was called with the Pending filter.
        _familyRepoMock.Verify(r => r.GetInvitationsByEmailAsync(
            It.IsAny<string>(),
            InvitationStatus.Pending,
            It.IsAny<PagedRequest>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoMatchingInvitations_ShouldReturnEmpty()
    {
        // Arrange
        var userId = UserId.New();
        var user = TestData.AnyUser();

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _familyRepoMock
            .Setup(r => r.GetInvitationsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<InvitationStatus?>(),
                It.IsAny<PagedRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult.Empty<(Family, Invitation)>(1, 20));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetMyInvitationsQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task Handle_ShouldForwardPaginationParamsToRepository()
    {
        // Arrange
        var userId = UserId.New();
        var user = TestData.AnyUser();

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _familyRepoMock
            .Setup(r => r.GetInvitationsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<InvitationStatus?>(),
                It.IsAny<PagedRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult.Empty<(Family, Invitation)>(3, 50));

        var sut = CreateSut();

        // Act
        await sut.Handle(
            new GetMyInvitationsQuery(Page: 3, PageSize: 50),
            CancellationToken.None);

        // Assert
        _familyRepoMock.Verify(r => r.GetInvitationsByEmailAsync(
            It.IsAny<string>(),
            It.IsAny<InvitationStatus?>(),
            It.Is<PagedRequest>(p => p.Page == 3 && p.PageSize == 50),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedUserNotFound_ShouldThrow()
    {
        // Arrange — token valid but user record missing (edge case: deleted account).
        var userId = UserId.New();

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.Handle(new GetMyInvitationsQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldLookUpInvitationsByUsersEmail()
    {
        // Arrange
        var userId = UserId.New();
        var userEmail = Email.Create("user@example.com");
        var user = TestData.AnyUser(email: userEmail);
        var family = TestData.AnyFamily(name: "Smith Family");
        var invitation = TestData.AnyInvitation(family: family, email: userEmail);
        
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _familyRepoMock
            .Setup(r => r.GetInvitationsByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<InvitationStatus?>(),
                It.IsAny<PagedRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<(Family, Invitation)>(
                new[] { (family, invitation) }, 1, 20, 1));

        var sut = CreateSut();

        // Act
        await sut.Handle(new GetMyInvitationsQuery(), CancellationToken.None);

        // Assert — repository called with the user's email.
        _familyRepoMock.Verify(r => r.GetInvitationsByEmailAsync(
            userEmail.Value,
            It.IsAny<InvitationStatus?>(),
            It.IsAny<PagedRequest>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}