namespace TourPlanner.Application.Common.Exceptions;

public sealed class TourPlannerConflictException : TourPlannerException
{
    public TourPlannerConflictException(string message) : base(message)
    {
    }
}

