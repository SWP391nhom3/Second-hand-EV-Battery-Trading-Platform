using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Favorites;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Favorite Service implementation
/// </summary>
public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        ILogger<FavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<FavoriteResponse>> AddToFavoritesAsync(Guid userId, Guid postId)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<FavoriteResponse>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            // 2. Validate post exists
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return BaseResponse<FavoriteResponse>.FailureResponse(
                    "Bài đăng không tồn tại");
            }

            // 3. Check if post is already in favorites
            var existingFavorite = await _favoriteRepository.GetByUserAndPostAsync(userId, postId);
            if (existingFavorite != null)
            {
                return BaseResponse<FavoriteResponse>.FailureResponse(
                    "Bài đăng đã có trong danh sách yêu thích");
            }

            // 4. Create favorite
            var favorite = new Favorite
            {
                UserId = userId,
                PostId = postId
            };

            await _favoriteRepository.CreateAsync(favorite);
            await _favoriteRepository.SaveChangesAsync();

            // 5. Load favorite with post details for response
            var createdFavorite = await _favoriteRepository.GetByIdAsync(favorite.Id);
            if (createdFavorite == null)
            {
                return BaseResponse<FavoriteResponse>.FailureResponse(
                    "Đã xảy ra lỗi khi tạo yêu thích");
            }

            var response = MapToResponse(createdFavorite);
            return BaseResponse<FavoriteResponse>.SuccessResponse(
                response,
                "Đã thêm vào danh sách yêu thích");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm vào yêu thích, UserId: {UserId}, PostId: {PostId}", userId, postId);
            return BaseResponse<FavoriteResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi thêm vào yêu thích");
        }
    }

    public async Task<BaseResponse> RemoveFromFavoritesAsync(Guid userId, Guid postId)
    {
        try
        {
            // 1. Find favorite with tracking for deletion
            var favorite = await _favoriteRepository.GetByUserAndPostForDeleteAsync(userId, postId);
            if (favorite == null)
            {
                return BaseResponse.FailureResponse(
                    "Bài đăng không có trong danh sách yêu thích");
            }

            // 2. Verify ownership
            if (favorite.UserId != userId)
            {
                return BaseResponse.FailureResponse(
                    "Bạn không có quyền xóa yêu thích này");
            }

            // 3. Delete favorite
            await _favoriteRepository.DeleteAsync(favorite);
            await _favoriteRepository.SaveChangesAsync();

            return BaseResponse.SuccessResponse(
                "Đã xóa khỏi danh sách yêu thích");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa khỏi yêu thích, UserId: {UserId}, PostId: {PostId}", userId, postId);
            return BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi xóa khỏi yêu thích");
        }
    }

    public async Task<PagedResponse<FavoriteResponse>> GetFavoritesAsync(Guid userId, FavoriteListRequest request)
    {
        try
        {
            // Validate request
            request.IsValid();

            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return PagedResponse<FavoriteResponse>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            // Get favorites
            var result = await _favoriteRepository.GetFavoritesByUserIdAsync(
                userId,
                request.PageNumber,
                request.PageSize,
                request.Keyword,
                request.CategoryId,
                request.Status,
                request.IsActive,
                request.IsSold,
                request.SortBy,
                request.SortDirection);

            // Map to response
            var responses = result.Items
                .Select(f => MapToResponse(f))
                .ToList();

            return PagedResponse<FavoriteResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách yêu thích thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách yêu thích, UserId: {UserId}", userId);
            return PagedResponse<FavoriteResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách yêu thích");
        }
    }

    public async Task<BaseResponse<bool>> IsFavoriteAsync(Guid userId, Guid postId)
    {
        try
        {
            var exists = await _favoriteRepository.ExistsAsync(userId, postId);
            return BaseResponse<bool>.SuccessResponse(
                exists,
                exists ? "Bài đăng đã có trong yêu thích" : "Bài đăng chưa có trong yêu thích");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi kiểm tra yêu thích, UserId: {UserId}, PostId: {PostId}", userId, postId);
            return BaseResponse<bool>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi kiểm tra yêu thích");
        }
    }

    /// <summary>
    /// Map Favorite entity to FavoriteResponse DTO
    /// </summary>
    private FavoriteResponse MapToResponse(Favorite favorite)
    {
        var response = new FavoriteResponse
        {
            FavoriteId = favorite.Id,
            PostId = favorite.PostId,
            UserId = favorite.UserId,
            CreatedAt = favorite.CreatedAt
        };

        // Map post info if available
        if (favorite.Post != null)
        {
            response.Post = new PostInfo
            {
                PostId = favorite.Post.Id,
                Title = favorite.Post.Title,
                Price = favorite.Post.Price,
                Location = favorite.Post.Location,
                Brand = favorite.Post.Brand,
                Model = favorite.Post.Model,
                Status = favorite.Post.Status,
                IsActive = favorite.Post.IsActive,
                IsSold = favorite.Post.IsSold,
                CreatedAt = favorite.Post.CreatedAt,
                ThumbnailImageUrl = favorite.Post.PostImages
                    ?.OrderBy(pi => pi.DisplayOrder)
                    .FirstOrDefault(pi => !pi.IsProof)?
                    .ImageUrl
            };
        }

        return response;
    }
}

