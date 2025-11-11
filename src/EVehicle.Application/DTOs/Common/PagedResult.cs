namespace EVehicle.Application.DTOs.Common;

/// <summary>
/// Class chứa kết quả phân trang từ repository/service
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của items</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Danh sách items
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Tổng số items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Số trang hiện tại
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Số lượng items trên mỗi trang
    /// </summary>
    public int PageSize { get; set; }

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
    /// Tạo PagedResult
    /// </summary>
    public static PagedResult<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

