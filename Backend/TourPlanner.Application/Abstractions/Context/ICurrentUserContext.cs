namespace TourPlanner.Application.Abstractions.Context;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }

    string UserName { get; }
}

