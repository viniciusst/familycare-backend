using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.Identity;

public sealed class UserRepository(FamilyCareDbContext dbContext) : IUserRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
        => _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AddAsync(user, cancellationToken);

    public void Update(User user)
        => _dbContext.Users.Update(user);
}
