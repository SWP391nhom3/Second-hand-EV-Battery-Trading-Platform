using EVehicle.Application.DTOs.Common;

namespace EVehicle.Application.DTOs.Orders;

/// <summary>
/// Request DTO cho tìm kiếm lịch sử thanh toán (UC30)
/// </summary>
public class PaymentSearchRequest : PagedRequest
{
    /// <summary>
    /// Loại thanh toán (PACKAGE, TRANSACTION) - null để lấy tất cả
    /// </summary>
    public string? PaymentType { get; set; }

    /// <summary>
    /// Trạng thái thanh toán (PENDING, SUCCESS, FAILED) - null để lấy tất cả
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Cổng thanh toán (PAYOS) - null để lấy tất cả
    /// </summary>
    public string? PaymentGateway { get; set; }

    /// <summary>
    /// Từ ngày (tùy chọn)
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Đến ngày (tùy chọn)
    /// </summary>
    public DateTime? ToDate { get; set; }
}


