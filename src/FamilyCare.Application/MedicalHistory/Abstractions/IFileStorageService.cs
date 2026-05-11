namespace FamilyCare.Application.MedicalHistory.Abstractions;

/// <summary>
/// Abstraction over file storage. Infrastructure provides Local or S3-compatible
/// implementations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Persists the file content and returns the resulting storage path
    /// (opaque string interpreted only by this storage backend).
    /// </summary>
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string mimeType,
        CancellationToken cancellationToken = default);

    /// <summary>Opens a stream to read a previously-saved file.</summary>
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
