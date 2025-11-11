namespace EVehicle.Application.DTOs.Packages;

/// <summary>
/// Request DTO cho việc tạo gói tin mới (UC48)
/// </summary>
public class PackageCreateRequest
{
    /// <summary>
    /// Tên gói tin (Basic, Premium, Luxury)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Giá gói tin (VND)
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Số lượng credits được cấp khi mua gói
    /// </summary>
    public int CreditsCount { get; set; }

    /// <summary>
    /// Mức độ ưu tiên (3=Luxury, 2=Premium, 1=Basic)
    /// </summary>
    public int PriorityLevel { get; set; }

    /// <summary>
    /// Số lượng ảnh tối đa cho phép
    /// </summary>
    public int MaxImages { get; set; } = 5;

    /// <summary>
    /// Trạng thái kích hoạt
    /// </summary>
    public bool IsActive { get; set; } = true;
}


