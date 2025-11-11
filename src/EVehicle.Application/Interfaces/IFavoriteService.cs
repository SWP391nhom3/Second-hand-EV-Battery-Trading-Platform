using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Favorites;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Favorite Service
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// UC18: Thêm bài đăng vào danh sách yêu thích
    /// </summary>
    Task<BaseResponse<FavoriteResponse>> AddToFavoritesAsync(Guid userId, Guid postId);

    /// <summary>
    /// UC19: Xóa bài đăng khỏi danh sách yêu thích
    /// </summary>
    Task<BaseResponse> RemoveFromFavoritesAsync(Guid userId, Guid postId);

    /// <summary>
    /// UC22: Xem danh sách yêu thích
    /// </summary>
    Task<PagedResponse<FavoriteResponse>> GetFavoritesAsync(Guid userId, FavoriteListRequest request);

    /// <summary>
    /// Kiểm tra xem bài đăng đã được thêm vào yêu thích chưa
    /// </summary>
    Task<BaseResponse<bool>> IsFavoriteAsync(Guid userId, Guid postId);
}

