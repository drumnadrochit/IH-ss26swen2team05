namespace TourPlanner.Application.Common.Exceptions;

public abstract class TourPlannerException : Exception
{
    protected TourPlannerException(string message) : base(message)
    {
    }
}

