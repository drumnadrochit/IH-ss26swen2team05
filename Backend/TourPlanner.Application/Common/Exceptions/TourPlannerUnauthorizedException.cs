namespace TourPlanner.Application.Common.Exceptions;

public sealed class TourPlannerUnauthorizedException : TourPlannerException
{
    public TourPlannerUnauthorizedException(string message) : base(message)
    {
    }
}

