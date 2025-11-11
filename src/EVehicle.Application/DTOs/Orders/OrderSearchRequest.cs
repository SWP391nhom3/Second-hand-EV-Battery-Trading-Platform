using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Orders;

/// <summary>
/// Request DTO cho tìm kiếm lịch sử giao dịch (UC05)
/// </summary>
public class OrderSearchRequest : PagedRequest
{
    /// <summary>
    /// Loại giao dịch (BUY, SELL) - null để lấy tất cả
    /// BUY: Lấy orders mà user là Buyer
    /// SELL: Lấy orders mà user là Seller
    /// </summary>
    public string? TransactionType { get; set; }

    /// <summary>
    /// Trạng thái đơn hàng (PENDING_PAYMENT, PAID, CONFIRMED, SHIPPING, DELIVERED, COMPLETED, CANCELLED) - null để lấy tất cả
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Từ ngày (tùy chọn)
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Đến ngày (tùy chọn)
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Tìm kiếm theo tiêu đề bài đăng (tùy chọn)
    /// </summary>
    public string? Keyword { get; set; }
}


