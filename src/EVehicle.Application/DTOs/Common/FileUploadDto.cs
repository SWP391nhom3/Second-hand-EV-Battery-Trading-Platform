namespace EVehicle.Application.DTOs.Common;

/// <summary>
/// DTO cho file upload (abstraction để tránh dependency vào ASP.NET Core)
/// </summary>
public class FileUploadDto
{
    /// <summary>
    /// Tên file
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Content type (MIME type)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File content dạng byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// File size (bytes)
    /// </summary>
    public long Length => Content.Length;
}

