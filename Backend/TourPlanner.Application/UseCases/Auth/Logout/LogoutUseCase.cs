using TourPlanner.Application.Abstractions.UseCases;
using TourPlanner.Application.Common;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Contracts.Persistence;

namespace TourPlanner.Application.UseCases.Auth.Logout;

public sealed class LogoutUseCase(
    IUserSessionRepository sessions,
    IUnitOfWork unitOfWork) : IUseCase<LogoutRequest>
{
    public async Task ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DEBUG LOGOUT] Received token from Scalar: '{request.RefreshToken}'");

        var session = await sessions.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
    
        if (session is null)
        {
            // 🚨 This will instantly alert you in the console if the database couldn't find the string!
            Console.WriteLine($"[DEBUG LOGOUT] FAILED: No active session found in the database for this token.");
            throw new Exception("Session token not found in database."); 
        }

        Console.WriteLine($"[DEBUG LOGOUT] SUCCESS: Found session for User {session.UserId}. Removing now...");
        sessions.Remove(session);
    
        var rowsAffected = await unitOfWork.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DEBUG LOGOUT] SaveChanges completed. Rows affected in DB: {rowsAffected}");
    }
}