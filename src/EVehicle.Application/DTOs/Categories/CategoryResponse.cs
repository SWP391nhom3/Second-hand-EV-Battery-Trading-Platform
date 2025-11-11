namespace EVehicle.Application.DTOs.Categories;

/// <summary>
/// Response DTO cho danh mục (dùng cho select2)
/// </summary>
public class CategoryResponse
{
    /// <summary>
    /// ID danh mục
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên danh mục
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Mã danh mục
    /// </summary>
    public string Code { get; set; } = string.Empty;
}

