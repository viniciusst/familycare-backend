using FamilyCare.Application.Authorization;
using FamilyCare.Application.FamilyManagement.Abstractions;
using FamilyCare.Application.MedicalHistory.Authorization;

namespace FamilyCare.Application.Tests.Common;

/// <summary>
/// Builds a real <see cref="MedicalAccessGuard"/> backed by mocks for its three
/// dependencies. Tests interact with the mocks to set up scenarios, but
/// exercise the real authorization flow (resolve membership, evaluate privacy).
/// </summary>
internal sealed class MedicalAccessGuardBuilder
{
    public Mock<ICurrentUserService> CurrentUser { get; } = new();
    public Mock<IMembershipResolver> MembershipResolver { get; } = new();
    public Mock<IPrivacyPolicyEvaluator> PrivacyEvaluator { get; } = new();

    public MedicalAccessGuard Build()
        => new(CurrentUser.Object, MembershipResolver.Object, PrivacyEvaluator.Object);

    /// <summary>
    /// Default permissive setup: any user is a member, can read and write anything.
    /// Tests override individual mocks for negative scenarios.
    /// </summary>
    public MedicalAccessGuardBuilder WithAllAccessGranted(
        UserId? userId = null,
        FamilyId? familyId = null,
        FamilyMemberId? requesterMemberId = null)
    {
        var uid = userId ?? UserId.New();
        var fid = familyId ?? FamilyId.New();
        var mid = requesterMemberId ?? FamilyMemberId.New();

        CurrentUser.Setup(c => c.RequireUserId()).Returns(uid);

        MembershipResolver
            .Setup(r => r.GetFamilyIdForMemberAsync(
                It.IsAny<FamilyMemberId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fid);

        // Returning a member via mock — we cheat by faking it as a typed FamilyMember
        // built through a real Family aggregate, since FamilyMember has no public ctor.
        var fakeFamily = Family.Create("FakeForTests", uid, "Owner", new DateOnly(1985, 1, 15));
        var member = fakeFamily.Members.Single();

        MembershipResolver
            .Setup(r => r.GetMembershipAsync(
                uid, fid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        PrivacyEvaluator
            .Setup(p => p.CanReadAsync(
                It.IsAny<FamilyId>(),
                It.IsAny<FamilyMemberId>(),
                It.IsAny<FamilyMemberId>(),
                It.IsAny<DataCategory>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        PrivacyEvaluator
            .Setup(p => p.CanWriteAsync(
                It.IsAny<FamilyId>(),
                It.IsAny<FamilyMemberId>(),
                It.IsAny<FamilyMemberId>(),
                It.IsAny<DataCategory>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        return this;
    }
}
