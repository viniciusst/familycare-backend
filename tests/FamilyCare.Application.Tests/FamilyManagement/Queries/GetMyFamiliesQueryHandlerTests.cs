using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Queries.GetMyFamilies;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Queries;

public class GetMyFamiliesQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private GetMyFamiliesQueryHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_ShouldReturnPagedSummaryDtos()
    {
        // Arrange
        var userId = UserId.New();
        var family1 = TestData.AnyFamily(ownerUserId: userId, name: "Family A");
        var family2 = TestData.AnyFamily(ownerUserId: userId, name: "Family B");

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _familyRepoMock
            .Setup(r => r.GetByUserIdAsync(
                userId, It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Family>(
                new[] { family1, family2 }, Page: 1, PageSize: 20, TotalCount: 2));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetMyFamiliesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task Handle_WhenNoFamilies_ShouldReturnEmpty()
    {
        var userId = UserId.New();

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(userId);
        _familyRepoMock
            .Setup(r => r.GetByUserIdAsync(
                userId, It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult.Empty<Family>(1, 20));

        var sut = CreateSut();

        var result = await sut.Handle(new GetMyFamiliesQuery(), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
