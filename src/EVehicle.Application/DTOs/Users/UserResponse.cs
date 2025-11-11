namespace EVehicle.Application.DTOs.Users;

/// <summary>
/// Response DTO cho thông tin người dùng (UC47)
/// </summary>
public class UserResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? IdNumber { get; set; }
    public string? IdFrontImageUrl { get; set; }
    public string? IdBackImageUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Thống kê
    public int TotalPosts { get; set; }
    public int ActivePosts { get; set; }
    public int TotalLeads { get; set; }
    public int TotalOrders { get; set; }
}

