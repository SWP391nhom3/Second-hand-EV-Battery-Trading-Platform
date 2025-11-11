namespace EVehicle.Application.DTOs.Common;

/// <summary>
/// Response cho các API có phân trang
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của items</typeparam>
public class PagedResponse<T> : BaseResponse<List<T>>
{
    /// <summary>
    /// Số trang hiện tại
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Số lượng items trên mỗi trang
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Tổng số items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Tổng số trang
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Có trang trước không
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Có trang sau không
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>
    /// Tạo PagedResponse thành công
    /// </summary>
    public static PagedResponse<T> SuccessResponse(
        List<T> data,
        int pageNumber,
        int pageSize,
        int totalCount,
        string message = "Thành công")
    {
        return new PagedResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Tạo PagedResponse thất bại
    /// </summary>
    public static new PagedResponse<T> FailureResponse(string message, List<string>? errors = null)
    {
        return new PagedResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            PageNumber = 0,
            PageSize = 0,
            TotalCount = 0,
            Timestamp = DateTime.UtcNow
        };
    }
}

