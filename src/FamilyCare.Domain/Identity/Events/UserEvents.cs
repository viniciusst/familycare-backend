using FamilyCare.Domain.Common;

namespace FamilyCare.Domain.Identity.Events;

public sealed record UserRegisteredEvent(UserId UserId, string Email, DateTime OccurredOn) : IDomainEvent;

public sealed record UserPasswordChangedEvent(UserId UserId, DateTime OccurredOn) : IDomainEvent;

public sealed record UserPreferredLanguageChangedEvent(
    UserId UserId,
    SupportedLanguage NewLanguage,
    DateTime OccurredOn) : IDomainEvent;
