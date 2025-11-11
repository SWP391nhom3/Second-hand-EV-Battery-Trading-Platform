namespace EVehicle.Application.DTOs.Common;

/// <summary>
/// Base class cho các request có phân trang
/// </summary>
public class PagedRequest
{
    /// <summary>
    /// Số trang (bắt đầu từ 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Số lượng items trên mỗi trang
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Trường để sắp xếp
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Hướng sắp xếp (asc hoặc desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Tính toán số bản ghi cần bỏ qua (skip)
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Validate request
    /// </summary>
    public virtual bool IsValid()
    {
        if (PageNumber < 1)
            PageNumber = 1;

        if (PageSize < 1)
            PageSize = 10;

        if (PageSize > 100)
            PageSize = 100; // Giới hạn tối đa 100 items/page

        if (string.IsNullOrWhiteSpace(SortDirection))
            SortDirection = "asc";

        SortDirection = SortDirection.ToLower();
        if (SortDirection != "asc" && SortDirection != "desc")
            SortDirection = "asc";

        return true;
    }
}

