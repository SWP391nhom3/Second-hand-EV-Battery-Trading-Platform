namespace EVehicle.Application.DTOs.Leads;

/// <summary>
/// Request DTO cho cập nhật trạng thái Lead (UC44)
/// </summary>
public class LeadStatusUpdateRequest
{
    /// <summary>
    /// Trạng thái mới (CONTACTED, SCHEDULED, SUCCESSFUL, FAILED)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Ghi chú của Staff
    /// </summary>
    public string? Notes { get; set; }
}

