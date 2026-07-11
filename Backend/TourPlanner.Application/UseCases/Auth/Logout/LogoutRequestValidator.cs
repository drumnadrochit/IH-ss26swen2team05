using FluentValidation;

namespace TourPlanner.Application.UseCases.Auth.Logout;

public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required to log out.")
            .NotNull();
    }
}