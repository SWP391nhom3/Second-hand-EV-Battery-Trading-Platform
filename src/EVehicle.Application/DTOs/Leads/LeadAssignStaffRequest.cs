namespace EVehicle.Application.DTOs.Leads;

/// <summary>
/// Request DTO cho việc Admin gán Staff cho Lead (UC46)
/// </summary>
public class LeadAssignStaffRequest
{
    /// <summary>
    /// ID của Staff được gán
    /// </summary>
    public Guid StaffId { get; set; }
}

