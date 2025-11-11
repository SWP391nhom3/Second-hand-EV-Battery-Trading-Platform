using EVehicle.Application.DTOs.Common;
using Microsoft.AspNetCore.Http;

namespace EVehicle.API.Helpers;

/// <summary>
/// Helper methods cho file operations
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Convert IFormFile sang FileUploadDto
    /// </summary>
    public static async Task<FileUploadDto?> ConvertToFileUploadDtoAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        return new FileUploadDto
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Content = memoryStream.ToArray()
        };
    }

    /// <summary>
    /// Convert list IFormFile sang List FileUploadDto
    /// </summary>
    public static async Task<List<FileUploadDto>> ConvertToFileUploadDtoListAsync(IEnumerable<IFormFile>? files)
    {
        var result = new List<FileUploadDto>();

        if (files == null)
            return result;

        foreach (var file in files)
        {
            if (file != null && file.Length > 0)
            {
                var dto = await ConvertToFileUploadDtoAsync(file);
                if (dto != null)
                {
                    result.Add(dto);
                }
            }
        }

        return result;
    }
}

