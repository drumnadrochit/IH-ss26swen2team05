using FluentValidation;
using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.CommonDtos.Auth;
using TourPlanner.Application.Contracts.Persistence;
using TourPlanner.Application.Contracts.Security;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.UseCases.Auth.Register;

public sealed class RegisterUseCase(
    IValidator<RegisterRequestDto> validator,
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUserSessionRepository sessions,
    IUnitOfWork unitOfWork) : IUseCase<RegisterRequestDto, AuthResponseDto>
{
    public async Task<AuthResponseDto> ExecuteAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await users.GetByUserNameAsync(request.UserName, cancellationToken);
        if (existing is not null)
        {
            throw new TourPlannerConflictException("The chosen user name is already registered.");
        }

        var user = User.Create(request.UserName, passwordHasher.Hash(request.Password));
        await users.AddAsync(user, cancellationToken);

        var tokens = await tokenService.GenerateTokenPairAsync(user, cancellationToken);
        await sessions.AddAsync(UserSession.Create(user.Id, tokens.RefreshToken, tokens.RefreshTokenExpiresAt), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(user.Id, user.UserName, tokens.AccessToken, tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
    }
}