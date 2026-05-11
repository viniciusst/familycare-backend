using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Identity.Abstractions;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.Identity.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(512);
    }
}

public sealed class LogoutCommandHandler(IAuthTokenService authTokenService)
    : IRequestHandler<LogoutCommand>
{
    private readonly IAuthTokenService _authTokenService = authTokenService;

    public Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        => _authTokenService.RevokeAsync(request.RefreshToken, "logout", cancellationToken);
}
