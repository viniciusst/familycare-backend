using System.Security.Claims;
using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace FamilyCare.Infrastructure.Identity.Services;

/// <summary>
/// Reads authentication info from the current HttpContext. Registered as scoped.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public UserId? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

            return Guid.TryParse(sub, out var guid)
                ? FamilyCare.Domain.Common.UserId.From(guid)
                : null;
        }
    }

    public UserId RequireUserId()
        => UserId ?? throw new UnauthenticatedException();
}
