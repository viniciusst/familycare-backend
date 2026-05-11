using FamilyCare.Application.MedicalHistory.Abstractions;
using Microsoft.Extensions.Options;

namespace FamilyCare.Infrastructure.Storage;

/// <summary>
/// File storage backed by the local filesystem. Stores files under
/// {LocalPath}/{yyyy}/{MM}/{guid}{ext} to avoid huge flat directories.
/// </summary>
public sealed class LocalFileStorageService(IOptions<StorageOptions> options) : IFileStorageService
{
    private readonly StorageOptions _options = options.Value;

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var now = DateTime.UtcNow;
        var relativeDir = Path.Combine(now.Year.ToString("D4", System.Globalization.CultureInfo.InvariantCulture),
                                       now.Month.ToString("D2", System.Globalization.CultureInfo.InvariantCulture));
        var ext = Path.GetExtension(fileName);
        var safeName = $"{Guid.NewGuid():N}{ext}";

        var absoluteDir = Path.Combine(_options.LocalPath, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var absolutePath = Path.Combine(absoluteDir, safeName);
        var relativePath = Path.Combine(relativeDir, safeName).Replace('\\', '/');

        await using var fileStream = new FileStream(
            absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken);

        return relativePath;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = Path.Combine(_options.LocalPath, storagePath);

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"File '{storagePath}' not found.", absolutePath);
        }

        Stream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = Path.Combine(_options.LocalPath, storagePath);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }
}
