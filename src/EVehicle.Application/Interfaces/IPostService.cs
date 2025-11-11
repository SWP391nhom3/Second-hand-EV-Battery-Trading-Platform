using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Post Service
/// </summary>
public interface IPostService
{
    Task<BaseResponse<PostResponse>> CreatePostAsync(Guid userId, PostCreateRequest request);
    Task<BaseResponse<PostDetailResponse>> GetPostByIdAsync(Guid postId);
    Task<BaseResponse<PostPublicDetailResponse>> GetPostByIdPublicAsync(Guid postId);
    Task<PagedResponse<PostResponse>> SearchApprovedPostsAsync(PostSearchRequest request);
    Task<PagedResponse<PendingPostResponse>> GetPendingPostsAsync(Guid adminId, PendingPostSearchRequest request);
    Task<PagedResponse<ApprovedRejectedPostResponse>> GetApprovedRejectedPostsAsync(Guid adminId, ApprovedRejectedPostSearchRequest request);
    Task<BaseResponse<PostDetailResponse>> ApprovePostAsync(Guid adminId, Guid postId, PostApproveRequest request);
    Task<BaseResponse> RejectPostAsync(Guid adminId, Guid postId, PostRejectRequest request);
    
    // UC07: Chỉnh sửa bài đăng
    Task<BaseResponse<PostResponse>> UpdatePostAsync(Guid userId, Guid postId, PostUpdateRequest request);
    
    // UC08: Xóa bài đăng
    Task<BaseResponse> DeletePostAsync(Guid userId, Guid postId);
    
    // UC09: Tạm ẩn/Hiện bài đăng
    Task<BaseResponse<PostResponse>> TogglePostActiveAsync(Guid userId, Guid postId, PostToggleActiveRequest request);
    
    // UC13: Xem danh sách bài đăng của mình
    Task<PagedResponse<PostResponse>> GetMyPostsAsync(Guid userId, MyPostsSearchRequest request);
    
    // UC20: So sánh Sản phẩm
    Task<BaseResponse<PostCompareResponse>> ComparePostsAsync(PostCompareRequest request);
}

