using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Queries.GetAppointmentsByMember;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.MedicalHistory.Queries;

public class GetAppointmentsByMemberQueryHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IAppointmentRepository> _repoMock = new();

    private GetAppointmentsByMemberQueryHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldReturnPagedDtos()
    {
        // Arrange
        var memberId = FamilyMemberId.New();
        var appointment = Appointment.Schedule(
            memberId, DateTime.UtcNow.AddDays(1), "Cardiology", "Dr. House");

        _guard.WithAllAccessGranted();
        _repoMock
            .Setup(r => r.GetByMemberAsync(
                memberId,
                It.IsAny<PagedRequest>(),
                It.IsAny<AppointmentStatus?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Appointment>(
                new[] { appointment }, 1, 20, 1));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetAppointmentsByMemberQuery(memberId),
            CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WhenReadForbidden_ShouldThrowForbidden()
    {
        _guard.WithAllAccessGranted();
        _guard.PrivacyEvaluator
            .Setup(p => p.CanReadAsync(
                It.IsAny<FamilyId>(),
                It.IsAny<FamilyMemberId>(),
                It.IsAny<FamilyMemberId>(),
                It.IsAny<DataCategory>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();

        await Assert.ThrowsAsync<ForbiddenException>(() => sut.Handle(
            new GetAppointmentsByMemberQuery(FamilyMemberId.New()),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNoResults_ShouldReturnEmptyPage()
    {
        _guard.WithAllAccessGranted();
        _repoMock
            .Setup(r => r.GetByMemberAsync(
                It.IsAny<FamilyMemberId>(),
                It.IsAny<PagedRequest>(),
                It.IsAny<AppointmentStatus?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult.Empty<Appointment>(1, 20));

        var sut = CreateSut();

        var result = await sut.Handle(
            new GetAppointmentsByMemberQuery(FamilyMemberId.New()),
            CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
