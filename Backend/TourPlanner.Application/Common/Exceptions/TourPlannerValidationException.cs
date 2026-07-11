namespace TourPlanner.Application.Common.Exceptions;

public sealed class TourPlannerValidationException : TourPlannerException
{
    public TourPlannerValidationException(string message) : base(message)
    {
    }
}

