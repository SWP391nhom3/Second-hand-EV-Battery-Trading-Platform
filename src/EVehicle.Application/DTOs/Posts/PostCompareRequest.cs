namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO cho UC20: So sánh Sản phẩm
/// </summary>
public class PostCompareRequest
{
    /// <summary>
    /// Danh sách ID bài đăng cần so sánh (tối đa 3-5 sản phẩm)
    /// </summary>
    public List<Guid> PostIds { get; set; } = new();
}


