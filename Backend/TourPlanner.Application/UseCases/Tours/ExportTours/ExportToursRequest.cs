namespace TourPlanner.Application.UseCases.Tours.ExportTours;

public sealed record ExportToursRequest(string FileName, string ContentType, byte[] Content);

