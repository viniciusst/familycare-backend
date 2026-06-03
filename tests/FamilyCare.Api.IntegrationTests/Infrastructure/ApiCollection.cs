using System.Diagnostics.CodeAnalysis;

namespace FamilyCare.Api.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit collection definition. Test classes that share the
/// <see cref="FamilyCareApiFactory"/> (and therefore the same Postgres
/// container) must be marked with <c>[Collection("Api")]</c>. This makes
/// xUnit instantiate the fixture once and reuse it.
/// </summary>
[CollectionDefinition("Api")]
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "xUnit's [CollectionDefinition] convention requires the 'Collection' suffix.")]
public sealed class ApiCollection : ICollectionFixture<FamilyCareApiFactory>
{
    // Marker only — xUnit reads the attributes above.
}