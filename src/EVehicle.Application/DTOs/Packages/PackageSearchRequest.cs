using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Packages;

/// <summary>
/// Request DTO cho việc tìm kiếm gói tin với phân trang (UC48)
/// </summary>
public class PackageSearchRequest : PagedRequest
{
    /// <summary>
    /// Lọc theo trạng thái kích hoạt (null = tất cả)
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Tìm kiếm theo tên gói tin
    /// </summary>
    public string? Keyword { get; set; }
}


