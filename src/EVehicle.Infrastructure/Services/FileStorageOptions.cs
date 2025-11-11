using EVehicle.Application.Interfaces;

namespace EVehicle.Infrastructure.Services;

/// <summary>
/// Implementation của IFileStorageOptions
/// </summary>
public class FileStorageOptions : IFileStorageOptions
{
    public string RootPath { get; }

    public FileStorageOptions(string rootPath)
    {
        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
    }
}

