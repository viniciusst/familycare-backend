using FamilyCare.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FamilyCare.Infrastructure.Persistence.Converters;

public sealed class RefreshTokenIdConverter : ValueConverter<RefreshTokenId, Guid>
{
    public RefreshTokenIdConverter() : base(id => id.Value, value => RefreshTokenId.From(value)) { }
}
