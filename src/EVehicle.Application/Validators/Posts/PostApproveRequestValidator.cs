using EVehicle.Application.DTOs.Posts;
using FluentValidation;

namespace EVehicle.Application.Validators.Posts;

/// <summary>
/// Validator cho PostApproveRequest
/// </summary>
public class PostApproveRequestValidator : AbstractValidator<PostApproveRequest>
{
    public PostApproveRequestValidator()
    {
        // Không cần validation gì, chỉ cần duyệt bài đăng
        // Staff sẽ được gán tự động khi có người yêu cầu tư vấn (Lead)
    }
}

