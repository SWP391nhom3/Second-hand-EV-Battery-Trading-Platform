using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVehicle.Infrastructure.Services;

/// <summary>
/// File Storage Service implementation (tạm thời lưu local, sau này có thể tích hợp với cloud storage)
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IFileStorageOptions _options;
    private readonly ILogger<FileStorageService> _logger;
    private const string UploadsFolder = "uploads";
    private const string ImagesFolder = "images";

    public FileStorageService(
        IFileStorageOptions options,
        ILogger<FileStorageService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(FileUploadDto file)
    {
        try
        {
            // Sử dụng root path từ options
            var rootPath = _options.RootPath;
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new InvalidOperationException("File storage root path chưa được cấu hình");
            }

            // Đảm bảo root path tồn tại
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            // Tạo thư mục nếu chưa tồn tại
            var uploadsPath = Path.Combine(rootPath, UploadsFolder, ImagesFolder);
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Tạo tên file unique
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Lưu file
            await File.WriteAllBytesAsync(filePath, file.Content);

            // Trả về URL (relative path từ wwwroot)
            var imageUrl = $"/{UploadsFolder}/{ImagesFolder}/{fileName}";
            
            _logger.LogInformation("Upload image thành công: {ImageUrl}", imageUrl);
            
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload image: {FileName}", file.FileName);
            throw;
        }
    }

    public Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return Task.FromResult(false);

            // Sử dụng root path từ options
            var rootPath = _options.RootPath;
            if (string.IsNullOrEmpty(rootPath))
            {
                return Task.FromResult(false);
            }

            // Lấy đường dẫn file từ URL (loại bỏ dấu / đầu tiên nếu có)
            var relativePath = imageUrl.TrimStart('/');
            var filePath = Path.Combine(rootPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Xóa image thành công: {ImageUrl}", imageUrl);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa image: {ImageUrl}", imageUrl);
            return Task.FromResult(false);
        }
    }
}

