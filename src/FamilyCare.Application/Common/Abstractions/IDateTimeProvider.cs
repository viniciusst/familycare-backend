namespace FamilyCare.Application.Common.Abstractions;

/// <summary>
/// Abstracts time access for testability (avoid sprinkling DateTime.UtcNow across the codebase).
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateOnly TodayUtc { get; }
}
