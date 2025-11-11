namespace EVehicle.Application.DTOs.Leads;

/// <summary>
/// Response DTO cho Lead
/// </summary>
public class LeadResponse
{
    /// <summary>
    /// ID Lead
    /// </summary>
    public Guid LeadId { get; set; }

    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Tiêu đề bài đăng
    /// </summary>
    public string PostTitle { get; set; } = string.Empty;

    /// <summary>
    /// ID người mua
    /// </summary>
    public Guid BuyerId { get; set; }

    /// <summary>
    /// Tên người mua
    /// </summary>
    public string BuyerName { get; set; } = string.Empty;

    /// <summary>
    /// Email người mua
    /// </summary>
    public string BuyerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại người mua
    /// </summary>
    public string? BuyerPhone { get; set; }

    /// <summary>
    /// Địa chỉ người mua
    /// </summary>
    public string? BuyerAddress { get; set; }

    /// <summary>
    /// ID người bán (từ Post.UserId)
    /// </summary>
    public Guid? SellerId { get; set; }

    /// <summary>
    /// Tên người bán
    /// </summary>
    public string? SellerName { get; set; }

    /// <summary>
    /// Email người bán
    /// </summary>
    public string? SellerEmail { get; set; }

    /// <summary>
    /// Số điện thoại người bán
    /// </summary>
    public string? SellerPhone { get; set; }

    /// <summary>
    /// Địa chỉ người bán
    /// </summary>
    public string? SellerAddress { get; set; }

    /// <summary>
    /// Thông tin sản phẩm - Thương hiệu
    /// </summary>
    public string? PostBrand { get; set; }

    /// <summary>
    /// Thông tin sản phẩm - Model
    /// </summary>
    public string? PostModel { get; set; }

    /// <summary>
    /// Thông tin sản phẩm - Mô tả
    /// </summary>
    public string? PostDescription { get; set; }

    /// <summary>
    /// Giá sản phẩm từ Post
    /// </summary>
    public decimal? PostPrice { get; set; }

    /// <summary>
    /// ID Staff được gán (NULL nếu chưa gán)
    /// </summary>
    public Guid? StaffId { get; set; }

    /// <summary>
    /// Tên Staff được gán
    /// </summary>
    public string? StaffName { get; set; }

    /// <summary>
    /// ID Admin gán Staff
    /// </summary>
    public Guid? AssignedBy { get; set; }

    /// <summary>
    /// Loại Lead (SCHEDULE_VIEW, AUCTION_WINNER)
    /// </summary>
    public string LeadType { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái (NEW, ASSIGNED, CONTACTED, SCHEDULED, SUCCESSFUL, FAILED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Giá cuối cùng (dùng cho đấu giá)
    /// </summary>
    public decimal? FinalPrice { get; set; }

    /// <summary>
    /// Thời gian được gán Staff
    /// </summary>
    public DateTime? AssignedAt { get; set; }

    /// <summary>
    /// Thời gian đóng Lead
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Ghi chú của Staff
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

