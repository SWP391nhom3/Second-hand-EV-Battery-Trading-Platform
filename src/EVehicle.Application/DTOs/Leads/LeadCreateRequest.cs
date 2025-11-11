namespace EVehicle.Application.DTOs.Leads;

/// <summary>
/// Request DTO cho việc tạo Lead
/// </summary>
public class LeadCreateRequest
{
    /// <summary>
    /// ID bài đăng
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Loại Lead (SCHEDULE_VIEW, AUCTION_WINNER)
    /// </summary>
    public string LeadType { get; set; } = "SCHEDULE_VIEW";
}

