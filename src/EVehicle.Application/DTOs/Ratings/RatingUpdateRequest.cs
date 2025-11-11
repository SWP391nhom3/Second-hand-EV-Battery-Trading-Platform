namespace EVehicle.Application.DTOs.Ratings;

/// <summary>
/// Request DTO cho UC33: Chỉnh sửa đánh giá (trong vòng 7 ngày)
/// </summary>
public class RatingUpdateRequest
{
    /// <summary>
    /// Điểm số mới (1-5 sao)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Nhận xét mới
    /// </summary>
    public string? Comment { get; set; }
}


