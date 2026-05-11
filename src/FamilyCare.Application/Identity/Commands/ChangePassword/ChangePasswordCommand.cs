using FamilyCare.Application.Common.Abstractions;

namespace FamilyCare.Application.Identity.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword)
    : ICommand;
