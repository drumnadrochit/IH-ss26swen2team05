using Microsoft.Extensions.Options;
using TourPlanner.Application.Contracts.Files;
using TourPlanner.Infrastructure.Options;

namespace TourPlanner.Infrastructure.Services;

public sealed class FileStorage(IOptions<StorageOptions> options) : IFileStorage
{
    public async Task<string> SaveFileAsync(string relativePath, byte[] content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path must not be empty.", nameof(relativePath));
        }

        var basePath = Path.GetFullPath(options.Value.BasePath);
        var fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The target path is outside the configured storage directory.");
        }

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
        return relativePath.Replace('\\', '/');
    }

    public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var basePath = Path.GetFullPath(options.Value.BasePath);
        var fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<byte[]?> ReadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var basePath = Path.GetFullPath(options.Value.BasePath);
        var fullPath = Path.GetFullPath(Path.Combine(basePath, path));
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            return Task.FromResult<byte[]?>(null);
        }

        var bytes = File.ReadAllBytes(fullPath);
        return Task.FromResult<byte[]?>(bytes);
    }
}

