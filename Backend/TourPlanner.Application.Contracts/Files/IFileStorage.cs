namespace TourPlanner.Application.Contracts.Files;

public interface IFileStorage
{
    Task<string> SaveFileAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);

    Task<byte[]?> ReadFileAsync(string path, CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);
}


