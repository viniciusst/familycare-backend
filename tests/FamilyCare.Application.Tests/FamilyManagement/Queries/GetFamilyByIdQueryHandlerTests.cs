using FamilyCare.Application.FamilyManagement.Queries.GetFamilyById;
using FamilyCare.Application.FamilyManagement.Queries.GetFamilyMembers;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Queries;

public class GetFamilyByIdQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private GetFamilyByIdQueryHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenRequesterIsMember_ShouldReturnDto()
    {
        // Arrange
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId, name: "Souza");

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act
        var dto = await sut.Handle(new GetFamilyByIdQuery(family.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Souza", dto.Name);
    }

    [Fact]
    public async Task Handle_WhenFamilyNotFound_ShouldThrowNotFound()
    {
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(UserId.New());
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<FamilyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Family?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Handle(new GetFamilyByIdQuery(FamilyId.New()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotMember_ShouldThrowForbidden()
    {
        var family = TestData.AnyFamily();
        var stranger = UserId.New();

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(stranger);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(new GetFamilyByIdQuery(family.Id), CancellationToken.None));
    }
}

public class GetFamilyMembersQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private GetFamilyMembersQueryHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenRequesterIsMember_ShouldReturnMembers()
    {
        // Arrange
        var (family, ownerUserId, _, _, _) = TestData.FamilyWithTwoMembers();

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act
        var members = await sut.Handle(new GetFamilyMembersQuery(family.Id), CancellationToken.None);

        // Assert
        Assert.Equal(2, members.Count);
    }

    [Fact]
    public async Task Handle_WhenNotMember_ShouldThrowForbidden()
    {
        var family = TestData.AnyFamily();
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(UserId.New());
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(new GetFamilyMembersQuery(family.Id), CancellationToken.None));
    }
}
