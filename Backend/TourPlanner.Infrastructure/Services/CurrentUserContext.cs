using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TourPlanner.Application.Abstractions;
using TourPlanner.Application.Abstractions.Context;

namespace TourPlanner.Infrastructure.Services;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid UserId
        => Guid.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : throw new InvalidOperationException("The current user id is not available.");

    public string UserName
        => httpContextAccessor.HttpContext?.User?.Identity?.Name
           ?? httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
           ?? string.Empty;
}

