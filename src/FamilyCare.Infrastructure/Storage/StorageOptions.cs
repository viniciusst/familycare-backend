namespace FamilyCare.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";

    public string LocalPath { get; set; } = "/app/storage";
}
