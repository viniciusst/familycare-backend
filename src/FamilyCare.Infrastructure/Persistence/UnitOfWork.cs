using FamilyCare.Application.Common.Abstractions;

namespace FamilyCare.Infrastructure.Persistence;

public sealed class UnitOfWork(FamilyCareDbContext dbContext) : IUnitOfWork
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
