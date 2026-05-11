using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Identity.Abstractions;
using FluentValidation;

namespace FamilyCare.Application.Identity.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthTokens>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
