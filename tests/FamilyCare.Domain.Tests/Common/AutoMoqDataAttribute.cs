using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace FamilyCare.Domain.Tests.Common;

/// <summary>
/// Combines AutoFixture's <see cref="AutoDataAttribute"/> with Moq integration,
/// allowing test parameters to be auto-populated with either anonymous data
/// or auto-mocked dependencies.
/// </summary>
public sealed class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() => new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true }))
    {
    }
}

/// <summary>
/// Inline auto data attribute: allows mixing fixed inline values with
/// auto-generated values in xUnit theory tests.
/// </summary>
public sealed class InlineAutoMoqDataAttribute(params object[] values)
    : InlineAutoDataAttribute(new AutoMoqDataAttribute(), values);
