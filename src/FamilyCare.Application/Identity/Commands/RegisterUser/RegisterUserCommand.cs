using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.Identity;

namespace FamilyCare.Application.Identity.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    SupportedLanguage PreferredLanguage)
    : ICommand<RegisterUserResponse>;

public sealed record RegisterUserResponse(UserId UserId, string Email);
