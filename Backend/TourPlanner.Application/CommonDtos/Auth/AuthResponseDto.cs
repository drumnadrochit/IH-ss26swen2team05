namespace TourPlanner.Application.CommonDtos.Auth;

public sealed record AuthResponseDto(Guid UserId, string UserName, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);


