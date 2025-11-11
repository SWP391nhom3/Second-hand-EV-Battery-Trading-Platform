using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho File Storage Service
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadImageAsync(FileUploadDto file);
    Task<bool> DeleteImageAsync(string imageUrl);
}

