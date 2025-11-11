namespace EVehicle.Application.DTOs.Posts;

/// <summary>
/// Request DTO cho việc từ chối bài đăng
/// UC12: Từ chối Bài đăng
/// </summary>
public class PostRejectRequest
{
    /// <summary>
    /// Lý do từ chối bài đăng
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;
}

