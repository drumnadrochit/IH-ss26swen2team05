namespace TourPlanner.Application.UseCases.Tours.ImportTours;

public sealed record ImportToursRequest (string FileName, byte[] Content);