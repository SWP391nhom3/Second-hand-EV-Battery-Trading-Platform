using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho AIPriceSuggestion entity
/// </summary>
public interface IAIPriceSuggestionRepository
{
    /// <summary>
    /// Tạo mới một AI price suggestion
    /// </summary>
    Task<AIPriceSuggestion> CreateAsync(AIPriceSuggestion suggestion);

    /// <summary>
    /// Lấy suggestion theo PostId (lấy suggestion mới nhất)
    /// </summary>
    Task<AIPriceSuggestion?> GetLatestByPostIdAsync(Guid postId);

    /// <summary>
    /// Lưu thay đổi vào database
    /// </summary>
    Task SaveChangesAsync();
}


