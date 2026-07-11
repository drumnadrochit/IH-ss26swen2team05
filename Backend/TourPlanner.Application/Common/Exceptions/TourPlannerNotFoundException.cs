namespace TourPlanner.Application.Common.Exceptions;

public sealed class TourPlannerNotFoundException : TourPlannerException
{
    public TourPlannerNotFoundException(string message) : base(message)
    {
    }
}

