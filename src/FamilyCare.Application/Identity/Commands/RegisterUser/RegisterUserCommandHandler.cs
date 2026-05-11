using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.Identity;
using MediatR;

namespace FamilyCare.Application.Identity.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<RegisterUserResponse> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException(
                "identity.email_already_registered",
                "An account with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.Register(email, passwordHash, request.PreferredLanguage);

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterUserResponse(user.Id, user.Email.Value);
    }
}
