using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.Identity;
using MediatR;

namespace FamilyCare.Application.Identity.Queries.GetMe;

public sealed record GetMeQuery : IQuery<UserProfileDto>;

public sealed record UserProfileDto(
    UserId Id,
    string Email,
    SupportedLanguage PreferredLanguage,
    DateTime CreatedAt);

public sealed class GetMeQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository)
    : IRequestHandler<GetMeQuery, UserProfileDto>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserProfileDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        return new UserProfileDto(user.Id, user.Email.Value, user.PreferredLanguage, user.CreatedAt);
    }
}
