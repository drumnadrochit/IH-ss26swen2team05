namespace TourPlanner.Application.UseCases.Tours.UploadTourImage;

public sealed record UploadTourImageRequest (Guid TourId, string FileName, byte[] FileContent);