using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Identity.Abstractions;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.Identity.Commands.RefreshTokens;

public sealed record RefreshTokensCommand(string RefreshToken) : ICommand<AuthTokens>;

public sealed class RefreshTokensCommandValidator : AbstractValidator<RefreshTokensCommand>
{
    public RefreshTokensCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(512);
    }
}

public sealed class RefreshTokensCommandHandler(IAuthTokenService authTokenService)
    : IRequestHandler<RefreshTokensCommand, AuthTokens>
{
    private readonly IAuthTokenService _authTokenService = authTokenService;

    public Task<AuthTokens> Handle(RefreshTokensCommand request, CancellationToken cancellationToken)
        => _authTokenService.RotateAsync(request.RefreshToken, cancellationToken);
}
