namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO để tạm ẩn/hiện bài đăng (UC09)
/// </summary>
public class PostToggleActiveRequest
{
    /// <summary>
    /// Trạng thái is_active mới (true = hiện, false = ẩn)
    /// </summary>
    public bool IsActive { get; set; }
}

