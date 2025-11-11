namespace EVehicle.Application.DTOs.Common;

/// <summary>
/// Base response class cho tất cả API responses
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của data</typeparam>
public class BaseResponse<T>
{
    /// <summary>
    /// Trạng thái thành công hay thất bại
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Thông điệp mô tả kết quả
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu trả về
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Danh sách lỗi (nếu có)
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Timestamp của response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tạo response thành công
    /// </summary>
    public static BaseResponse<T> SuccessResponse(T data, string message = "Thành công")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Tạo response thất bại
    /// </summary>
    public static BaseResponse<T> FailureResponse(string message, List<string>? errors = null)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Tạo response thất bại từ exception
    /// </summary>
    public static BaseResponse<T> FailureResponse(Exception ex, string message = "Đã xảy ra lỗi")
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { ex.Message },
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Base response không có data
/// </summary>
public class BaseResponse : BaseResponse<object>
{
    /// <summary>
    /// Tạo response thành công không có data
    /// </summary>
    public static BaseResponse SuccessResponse(string message = "Thành công")
    {
        return new BaseResponse
        {
            Success = true,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Tạo response thất bại
    /// </summary>
    public static new BaseResponse FailureResponse(string message, List<string>? errors = null)
    {
        return new BaseResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Tạo response thất bại từ exception
    /// </summary>
    public static new BaseResponse FailureResponse(Exception ex, string message = "Đã xảy ra lỗi")
    {
        return new BaseResponse
        {
            Success = false,
            Message = message,
            Errors = new List<string> { ex.Message },
            Timestamp = DateTime.UtcNow
        };
    }
}

