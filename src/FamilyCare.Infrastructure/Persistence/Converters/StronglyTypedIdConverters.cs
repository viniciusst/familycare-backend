using FamilyCare.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FamilyCare.Infrastructure.Persistence.Converters;

/// <summary>
/// Converters between strong-typed IDs (record structs over Guid) and Guid
/// for EF Core persistence. One per ID — verbose but explicit and trivial.
/// </summary>
public sealed class UserIdConverter : ValueConverter<UserId, Guid>
{
    public UserIdConverter() : base(id => id.Value, value => UserId.From(value)) { }
}

public sealed class FamilyIdConverter : ValueConverter<FamilyId, Guid>
{
    public FamilyIdConverter() : base(id => id.Value, value => FamilyId.From(value)) { }
}

public sealed class FamilyMemberIdConverter : ValueConverter<FamilyMemberId, Guid>
{
    public FamilyMemberIdConverter() : base(id => id.Value, value => FamilyMemberId.From(value)) { }
}

public sealed class InvitationIdConverter : ValueConverter<InvitationId, Guid>
{
    public InvitationIdConverter() : base(id => id.Value, value => InvitationId.From(value)) { }
}

public sealed class PrivacyRuleIdConverter : ValueConverter<PrivacyRuleId, Guid>
{
    public PrivacyRuleIdConverter() : base(id => id.Value, value => PrivacyRuleId.From(value)) { }
}

public sealed class AppointmentIdConverter : ValueConverter<AppointmentId, Guid>
{
    public AppointmentIdConverter() : base(id => id.Value, value => AppointmentId.From(value)) { }
}

public sealed class ExamIdConverter : ValueConverter<ExamId, Guid>
{
    public ExamIdConverter() : base(id => id.Value, value => ExamId.From(value)) { }
}

public sealed class VaccineIdConverter : ValueConverter<VaccineId, Guid>
{
    public VaccineIdConverter() : base(id => id.Value, value => VaccineId.From(value)) { }
}

public sealed class AllergyIdConverter : ValueConverter<AllergyId, Guid>
{
    public AllergyIdConverter() : base(id => id.Value, value => AllergyId.From(value)) { }
}

public sealed class ChronicConditionIdConverter : ValueConverter<ChronicConditionId, Guid>
{
    public ChronicConditionIdConverter() : base(id => id.Value, value => ChronicConditionId.From(value)) { }
}

public sealed class AttachmentIdConverter : ValueConverter<AttachmentId, Guid>
{
    public AttachmentIdConverter() : base(id => id.Value, value => AttachmentId.From(value)) { }
}
