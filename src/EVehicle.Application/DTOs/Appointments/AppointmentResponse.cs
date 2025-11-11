namespace EVehicle.Application.DTOs.Appointments;

/// <summary>
/// Response DTO cho Appointment
/// </summary>
public class AppointmentResponse
{
    /// <summary>
    /// ID Appointment
    /// </summary>
    public Guid AppointmentId { get; set; }

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
    /// ID người bán
    /// </summary>
    public Guid SellerId { get; set; }

    /// <summary>
    /// Tên người bán
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// Email người bán
    /// </summary>
    public string SellerEmail { get; set; } = string.Empty;

    /// <summary>
    /// ID Staff
    /// </summary>
    public Guid StaffId { get; set; }

    /// <summary>
    /// Tên Staff
    /// </summary>
    public string StaffName { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian bắt đầu
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Thời gian kết thúc
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Địa điểm
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Trạng thái (CONFIRMED, CANCELED, COMPLETED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

