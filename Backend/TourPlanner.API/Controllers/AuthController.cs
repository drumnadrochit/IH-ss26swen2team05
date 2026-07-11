using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.CommonDtos.Auth;
using TourPlanner.Application.UseCases.Auth.Login;
using TourPlanner.Application.UseCases.Auth.Logout;
using TourPlanner.Application.UseCases.Auth.Refresh;
using TourPlanner.Application.UseCases.Auth.Register;

namespace TourPlanner.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IUseCase<RegisterRequestDto, AuthResponseDto> registerUseCase,
    IUseCase<LoginRequestDto, AuthResponseDto> loginUseCase,
    IUseCase<RefreshTokenRequestDto, AuthResponseDto> refreshUseCase
    ) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
        => Ok(await registerUseCase.ExecuteAsync(request, cancellationToken));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        => Ok(await loginUseCase.ExecuteAsync(request, cancellationToken));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        => Ok(await refreshUseCase.ExecuteAsync(request, cancellationToken));
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        [FromServices] IUseCase<LogoutRequest> logoutUseCase,
        CancellationToken cancellationToken)
    {
        await logoutUseCase.ExecuteAsync(request, cancellationToken);
        return NoContent();
    }
}

