namespace FamilyCare.Domain.FamilyManagement;

public enum Role
{
    Owner = 1,
    Admin = 2,
    Adult = 3,
    Minor = 4,
    Caregiver = 5
}

public enum RelationshipType
{
    Self = 1,
    Spouse = 2,
    Child = 3,
    Parent = 4,
    Sibling = 5,
    Grandparent = 6,
    Grandchild = 7,
    Other = 99
}

public enum InvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3,
    Expired = 4,
    Revoked = 5
}

public enum VisibilityScope
{
    Private = 1,
    FamilyAdmins = 2,
    AllFamily = 3,
    Custom = 4
}

/// <summary>Bounded context categories used by privacy rules.</summary>
public enum DataCategory
{
    MedicalHistory = 1,
    Medications = 2,
    Wellbeing = 3,
    Activity = 4,
    Nutrition = 5
}
