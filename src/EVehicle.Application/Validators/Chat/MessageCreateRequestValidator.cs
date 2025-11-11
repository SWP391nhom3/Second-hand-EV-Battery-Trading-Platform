using EVehicle.Application.DTOs.Chat;
using EVehicle.Application.DTOs.Common;
using FluentValidation;

namespace EVehicle.Application.Validators.Chat;

/// <summary>
/// Validator cho MessageCreateRequest
/// </summary>
public class MessageCreateRequestValidator : AbstractValidator<MessageCreateRequest>
{
    public MessageCreateRequestValidator()
    {
        // Nếu có RoomId thì không cần PostId
        RuleFor(x => x.PostId)
            .NotNull()
            .When(x => !x.RoomId.HasValue)
            .WithMessage("Phải cung cấp PostId hoặc RoomId");

        RuleFor(x => x.RoomId)
            .NotNull()
            .When(x => !x.PostId.HasValue)
            .WithMessage("Phải cung cấp PostId hoặc RoomId");

        RuleFor(x => x.MessageType)
            .NotEmpty()
            .WithMessage("Loại tin nhắn không được để trống")
            .Must(type => type == "TEXT" || type == "IMAGE" || type == "FILE")
            .WithMessage("Loại tin nhắn không hợp lệ. Chỉ chấp nhận TEXT, IMAGE, hoặc FILE");

        // Nếu là TEXT thì phải có Content
        RuleFor(x => x.Content)
            .NotEmpty()
            .When(x => x.MessageType == "TEXT")
            .WithMessage("Nội dung tin nhắn không được để trống cho loại TEXT")
            .MaximumLength(5000)
            .When(x => x.MessageType == "TEXT")
            .WithMessage("Nội dung tin nhắn không được vượt quá 5000 ký tự");

        // Nếu là IMAGE hoặc FILE thì phải có File
        RuleFor(x => x.File)
            .NotNull()
            .When(x => x.MessageType == "IMAGE" || x.MessageType == "FILE")
            .WithMessage("Phải có file đính kèm cho loại IMAGE hoặc FILE");

        // Validate file nếu có
        RuleFor(x => x.File)
            .Must(BeValidImage)
            .When(x => x.MessageType == "IMAGE" && x.File != null)
            .WithMessage("File ảnh không hợp lệ. Chỉ chấp nhận file JPG, PNG, JPEG với kích thước tối đa 10MB");

        RuleFor(x => x.File)
            .Must(BeValidFile)
            .When(x => x.MessageType == "FILE" && x.File != null)
            .WithMessage("File không hợp lệ. Kích thước tối đa 50MB");
    }

    private bool BeValidImage(FileUploadDto? file)
    {
        if (file == null)
            return false;

        // Kiểm tra extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return false;

        // Kiểm tra kích thước (10MB)
        const long maxSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxSize)
            return false;

        // Kiểm tra content type
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return false;

        return true;
    }

    private bool BeValidFile(FileUploadDto? file)
    {
        if (file == null)
            return false;

        // Kiểm tra kích thước (50MB)
        const long maxSize = 50 * 1024 * 1024; // 50MB
        if (file.Length > maxSize)
            return false;

        return true;
    }
}

