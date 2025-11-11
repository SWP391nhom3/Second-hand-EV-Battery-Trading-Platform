using EVehicle.Application.DTOs.Chat;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using EVehicle.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EVehicle.Application.Services;

/// <summary>
/// Chat Service implementation
/// </summary>
public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IChatRepository chatRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        ILeadRepository leadRepository,
        IFileStorageService fileStorageService,
        INotificationRepository notificationRepository,
        ILogger<ChatService> logger)
    {
        _chatRepository = chatRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _leadRepository = leadRepository;
        _fileStorageService = fileStorageService;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<BaseResponse<MessageResponse>> SendMessageAsync(
        Guid userId, 
        MessageCreateRequest request)
    {
        try
        {
            // 1. Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<MessageResponse>.FailureResponse(
                    "Người dùng không tồn tại");
            }

            ChatRoom? room = null;

            // 2. Tìm hoặc tạo phòng chat
            if (request.RoomId.HasValue)
            {
                // Lấy phòng chat đã tồn tại
                room = await _chatRepository.GetRoomByIdAsync(request.RoomId.Value);
                if (room == null)
                {
                    return BaseResponse<MessageResponse>.FailureResponse(
                        "Phòng chat không tồn tại");
                }

                // Kiểm tra user có quyền gửi tin nhắn trong phòng chat này không
                if (room.BuyerId != userId && room.SellerId != userId && room.StaffId != userId)
                {
                    return BaseResponse<MessageResponse>.FailureResponse(
                        "Bạn không có quyền gửi tin nhắn trong phòng chat này");
                }
            }
            else if (request.PostId.HasValue)
            {
                // Tìm phòng chat đã tồn tại hoặc tạo mới
                var post = await _postRepository.GetByIdAsync(request.PostId.Value);
                if (post == null)
                {
                    return BaseResponse<MessageResponse>.FailureResponse(
                        "Bài đăng không tồn tại");
                }

                // Kiểm tra user không phải là người bán
                if (post.UserId == userId)
                {
                    return BaseResponse<MessageResponse>.FailureResponse(
                        "Bạn không thể tự nhắn tin cho chính mình");
                }

                // Tìm phòng chat đã tồn tại
                room = await _chatRepository.GetRoomByPostAndBuyerAsync(
                    request.PostId.Value, 
                    userId);

                if (room == null)
                {
                    // Tạo phòng chat mới
                    room = new ChatRoom
                    {
                        Id = Guid.NewGuid(),
                        PostId = request.PostId.Value,
                        BuyerId = userId,
                        SellerId = post.UserId,
                        StaffId = null, // Sẽ được thêm sau khi Admin gán Staff cho Lead
                        CreatedAt = DateTime.UtcNow
                    };

                    await _chatRepository.CreateRoomAsync(room);
                }
            }
            else
            {
                return BaseResponse<MessageResponse>.FailureResponse(
                    "Phải cung cấp RoomId hoặc PostId");
            }

            // 3. Xử lý nội dung tin nhắn
            string content = request.Content ?? string.Empty;
            string messageType = request.MessageType;

            // Nếu là IMAGE hoặc FILE, upload file
            if ((messageType == "IMAGE" || messageType == "FILE") && request.File != null)
            {
                if (messageType == "IMAGE")
                {
                    var imageUrl = await _fileStorageService.UploadImageAsync(request.File);
                    content = imageUrl; // Lưu URL ảnh vào content
                }
                else
                {
                    // TODO: Implement file upload service (nếu cần)
                    return BaseResponse<MessageResponse>.FailureResponse(
                        "Tính năng upload file chưa được hỗ trợ");
                }
            }

            // 4. Tạo tin nhắn
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                UserId = userId,
                Content = content,
                MessageType = messageType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _chatRepository.CreateMessageAsync(message);

            // 5. Cập nhật LastMessageAt của phòng chat
            room.LastMessageAt = DateTime.UtcNow;
            await _chatRepository.UpdateRoomAsync(room);

            // 6. Tạo thông báo cho người nhận (người không phải người gửi)
            var recipients = new List<Guid>();
            if (room.BuyerId != userId)
                recipients.Add(room.BuyerId);
            if (room.SellerId != userId)
                recipients.Add(room.SellerId);
            if (room.StaffId.HasValue && room.StaffId.Value != userId)
                recipients.Add(room.StaffId.Value);

            foreach (var recipientId in recipients)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = recipientId,
                    NotificationType = "NEW_MESSAGE",
                    Title = "Tin nhắn mới",
                    Content = $"Bạn có tin nhắn mới từ {user.FullName ?? user.Email}",
                    RelatedId = room.Id,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.CreateAsync(notification);
            }

            await _chatRepository.SaveChangesAsync();
            await _notificationRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Gửi tin nhắn thành công, MessageId: {MessageId}, RoomId: {RoomId}, UserId: {UserId}",
                message.Id, room.Id, userId);

            // 7. Map to response
            var response = await MapToMessageResponseAsync(message);

            return BaseResponse<MessageResponse>.SuccessResponse(
                response,
                "Gửi tin nhắn thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi tin nhắn, UserId: {UserId}", userId);
            return BaseResponse<MessageResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi gửi tin nhắn");
        }
    }

    public async Task<PagedResponse<MessageResponse>> GetChatHistoryAsync(
        Guid userId, 
        ChatHistoryRequest request)
    {
        try
        {
            request.IsValid();

            // 1. Kiểm tra phòng chat tồn tại và user có quyền truy cập
            var room = await _chatRepository.GetRoomByIdAsync(request.RoomId);
            if (room == null)
            {
                return PagedResponse<MessageResponse>.FailureResponse(
                    "Phòng chat không tồn tại");
            }

            if (room.BuyerId != userId && room.SellerId != userId && room.StaffId != userId)
            {
                return PagedResponse<MessageResponse>.FailureResponse(
                    "Bạn không có quyền truy cập phòng chat này");
            }

            // 2. Lấy lịch sử tin nhắn
            var result = await _chatRepository.GetMessagesByRoomIdAsync(
                request.RoomId,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDirection);

            // 3. Đánh dấu tin nhắn đã đọc
            await _chatRepository.MarkMessagesAsReadAsync(request.RoomId, userId);
            await _chatRepository.SaveChangesAsync();

            // 4. Map to response
            var responses = new List<MessageResponse>();
            foreach (var message in result.Items)
            {
                var response = await MapToMessageResponseAsync(message);
                responses.Add(response);
            }

            return PagedResponse<MessageResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy lịch sử chat thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử chat, UserId: {UserId}, RoomId: {RoomId}",
                userId, request.RoomId);
            return PagedResponse<MessageResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy lịch sử chat");
        }
    }

    public async Task<PagedResponse<ChatRoomResponse>> GetChatRoomsAsync(
        Guid userId, 
        ChatRoomsListRequest request)
    {
        try
        {
            request.IsValid();

            var result = await _chatRepository.GetRoomsByUserIdAsync(
                userId,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDirection);

            var responses = new List<ChatRoomResponse>();
            foreach (var room in result.Items)
            {
                var response = await MapToChatRoomResponseAsync(room, userId);
                responses.Add(response);
            }

            return PagedResponse<ChatRoomResponse>.SuccessResponse(
                responses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Lấy danh sách phòng chat thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách phòng chat, UserId: {UserId}", userId);
            return PagedResponse<ChatRoomResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách phòng chat");
        }
    }

    public async Task<BaseResponse<ChatRoomResponse>> GetChatRoomAsync(
        Guid userId, 
        Guid roomId)
    {
        try
        {
            var room = await _chatRepository.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Phòng chat không tồn tại");
            }

            if (room.BuyerId != userId && room.SellerId != userId && room.StaffId != userId)
            {
                return BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Bạn không có quyền truy cập phòng chat này");
            }

            var response = await MapToChatRoomResponseAsync(room, userId);

            return BaseResponse<ChatRoomResponse>.SuccessResponse(
                response,
                "Lấy thông tin phòng chat thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin phòng chat, UserId: {UserId}, RoomId: {RoomId}",
                userId, roomId);
            return BaseResponse<ChatRoomResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy thông tin phòng chat");
        }
    }

    public async Task<BaseResponse<ChatRoomResponse>> CreateRoomForLeadAsync(
        Guid staffId,
        ChatRoomCreateRequest request)
    {
        try
        {
            // 1. Lấy thông tin lead
            var lead = await _leadRepository.GetByIdWithDetailsAsync(request.LeadId);
            if (lead == null)
            {
                return BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Lead không tồn tại");
            }

            if (!lead.StaffId.HasValue)
            {
                return BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Lead này chưa được gán Staff");
            }

            if (lead.StaffId.Value != staffId)
            {
                return BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Bạn không có quyền tạo phòng chat cho Lead này");
            }

            if (lead.Post == null)
            {
                return BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Không tìm thấy thông tin bài đăng của Lead");
            }

            var sellerId = lead.Post.UserId;
            if (sellerId == Guid.Empty)
            {
                return BaseResponse<ChatRoomResponse>.FailureResponse(
                    "Không tìm thấy thông tin người bán của bài đăng");
            }

            // 2. Kiểm tra phòng chat đã tồn tại theo Lead
            var existingRoom = await _chatRepository.GetRoomByLeadIdAsync(request.LeadId);
            ChatRoom room;
            string responseMessage;
            bool requiresSave = false;

            if (existingRoom != null)
            {
                // Bổ sung Staff hoặc LeadId nếu thiếu
                if (!existingRoom.StaffId.HasValue || existingRoom.StaffId.Value != lead.StaffId.Value)
                {
                    existingRoom.StaffId = lead.StaffId;
                    requiresSave = true;
                }

                if (!existingRoom.LeadId.HasValue)
                {
                    existingRoom.LeadId = lead.Id;
                    requiresSave = true;
                }

                if (requiresSave)
                {
                    await _chatRepository.UpdateRoomAsync(existingRoom);
                    await _chatRepository.SaveChangesAsync();
                }

                room = existingRoom;
                responseMessage = "Phòng chat đã tồn tại";
            }
            else
            {
                // 3. Kiểm tra phòng chat giữa Buyer và Seller theo Post (chưa có Lead)
                var roomByPostAndBuyer = await _chatRepository.GetRoomByPostAndBuyerAsync(lead.PostId, lead.BuyerId);
                if (roomByPostAndBuyer != null)
                {
                    roomByPostAndBuyer.LeadId = lead.Id;
                    roomByPostAndBuyer.StaffId = lead.StaffId;
                    await _chatRepository.UpdateRoomAsync(roomByPostAndBuyer);
                    await _chatRepository.SaveChangesAsync();

                    room = roomByPostAndBuyer;
                    responseMessage = "Đã gắn Staff vào phòng chat sẵn có";
                }
                else
                {
                    // 4. Tạo phòng chat mới
                    room = new ChatRoom
                    {
                        LeadId = lead.Id,
                        PostId = lead.PostId,
                        BuyerId = lead.BuyerId,
                        SellerId = sellerId,
                        StaffId = lead.StaffId
                    };

                    await _chatRepository.CreateRoomAsync(room);
                    await _chatRepository.SaveChangesAsync();

                    responseMessage = "Tạo phòng chat thành công";
                }
            }

            // 5. Trả về thông tin phòng chat
            var roomResponse = await MapToChatRoomResponseAsync(room, staffId);

            return BaseResponse<ChatRoomResponse>.SuccessResponse(
                roomResponse,
                responseMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Lỗi khi tạo phòng chat cho LeadId: {LeadId}, StaffId: {StaffId}",
                request.LeadId, staffId);

            return BaseResponse<ChatRoomResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo phòng chat");
        }
    }

    private async Task<MessageResponse> MapToMessageResponseAsync(ChatMessage message)
    {
        var user = await _userRepository.GetByIdAsync(message.UserId);

        return new MessageResponse
        {
            MessageId = message.Id,
            RoomId = message.RoomId,
            UserId = message.UserId,
            SenderName = user?.FullName ?? user?.Email ?? "Người dùng",
            SenderAvatar = user?.AvatarUrl,
            Content = message.Content,
            MessageType = message.MessageType,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }

    private async Task<ChatRoomResponse> MapToChatRoomResponseAsync(
        ChatRoom room, 
        Guid currentUserId)
    {
        var buyer = await _userRepository.GetByIdAsync(room.BuyerId);
        var seller = await _userRepository.GetByIdAsync(room.SellerId);
        User? staff = null;
        if (room.StaffId.HasValue)
        {
            staff = await _userRepository.GetByIdAsync(room.StaffId.Value);
        }

        // Lấy tin nhắn cuối cùng
        MessageResponse? lastMessage = null;
        if (room.ChatMessages.Any())
        {
            var lastMessageEntity = room.ChatMessages
                .OrderByDescending(m => m.CreatedAt)
                .First();
            lastMessage = await MapToMessageResponseAsync(lastMessageEntity);
        }

        // Đếm số tin nhắn chưa đọc
        var unreadCount = await _chatRepository.GetUnreadCountAsync(room.Id, currentUserId);

        // Lấy thumbnail của bài đăng
        var post = await _postRepository.GetByIdWithImagesAsync(room.PostId);
        var postThumbnail = post?.PostImages
            .FirstOrDefault(img => img.IsThumbnail)?.ImageUrl;

        return new ChatRoomResponse
        {
            RoomId = room.Id,
            PostId = room.PostId,
            PostTitle = post?.Title ?? string.Empty,
            PostThumbnail = postThumbnail,
            BuyerId = room.BuyerId,
            BuyerName = buyer?.FullName ?? buyer?.Email ?? "Người mua",
            BuyerAvatar = buyer?.AvatarUrl,
            SellerId = room.SellerId,
            SellerName = seller?.FullName ?? seller?.Email ?? "Người bán",
            SellerAvatar = seller?.AvatarUrl,
            StaffId = room.StaffId,
            StaffName = staff?.FullName ?? staff?.Email,
            StaffAvatar = staff?.AvatarUrl,
            LastMessage = lastMessage,
            UnreadCount = unreadCount,
            LastMessageAt = room.LastMessageAt,
            CreatedAt = room.CreatedAt
        };
    }
}

