namespace EVehicle.Application.Interfaces;

/// <summary>
/// Options cho File Storage Service (abstraction để tránh dependency vào ASP.NET Core)
/// </summary>
public interface IFileStorageOptions
{
    /// <summary>
    /// Root path để lưu files (ví dụ: wwwroot path)
    /// </summary>
    string RootPath { get; }
}

