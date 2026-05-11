namespace FamilyCare.Domain.Common;

// Strong-typed IDs: prevent accidentally mixing a UserId with a FamilyMemberId.
// Stored internally as Guid, but the type system treats them as distinct.
//
// Each ID exposes:
//   - New()        → generates a new one
//   - From(Guid)   → wraps an existing Guid (e.g. coming from the DB)
//   - Empty        → the default sentinel value
//   - Value        → underlying Guid (mostly for persistence)

public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);
    public static UserId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct FamilyId(Guid Value)
{
    public static FamilyId New() => new(Guid.NewGuid());
    public static FamilyId From(Guid value) => new(value);
    public static FamilyId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct FamilyMemberId(Guid Value)
{
    public static FamilyMemberId New() => new(Guid.NewGuid());
    public static FamilyMemberId From(Guid value) => new(value);
    public static FamilyMemberId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct InvitationId(Guid Value)
{
    public static InvitationId New() => new(Guid.NewGuid());
    public static InvitationId From(Guid value) => new(value);
    public static InvitationId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct PrivacyRuleId(Guid Value)
{
    public static PrivacyRuleId New() => new(Guid.NewGuid());
    public static PrivacyRuleId From(Guid value) => new(value);
    public static PrivacyRuleId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct AppointmentId(Guid Value)
{
    public static AppointmentId New() => new(Guid.NewGuid());
    public static AppointmentId From(Guid value) => new(value);
    public static AppointmentId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct ExamId(Guid Value)
{
    public static ExamId New() => new(Guid.NewGuid());
    public static ExamId From(Guid value) => new(value);
    public static ExamId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct VaccineId(Guid Value)
{
    public static VaccineId New() => new(Guid.NewGuid());
    public static VaccineId From(Guid value) => new(value);
    public static VaccineId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct AllergyId(Guid Value)
{
    public static AllergyId New() => new(Guid.NewGuid());
    public static AllergyId From(Guid value) => new(value);
    public static AllergyId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct ChronicConditionId(Guid Value)
{
    public static ChronicConditionId New() => new(Guid.NewGuid());
    public static ChronicConditionId From(Guid value) => new(value);
    public static ChronicConditionId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public readonly record struct AttachmentId(Guid Value)
{
    public static AttachmentId New() => new(Guid.NewGuid());
    public static AttachmentId From(Guid value) => new(value);
    public static AttachmentId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}
