using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.Identity;
using MediatR;

namespace FamilyCare.Application.Identity.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAuthTokenService authTokenService)
    : IRequestHandler<LoginCommand, AuthTokens>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IAuthTokenService _authTokenService = authTokenService;

    public async Task<AuthTokens> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Same error for "user not found" and "wrong password" to avoid user enumeration.
            throw new ForbiddenException("Invalid email or password.");
        }

        return await _authTokenService.IssueTokensAsync(user, cancellationToken);
    }
}
