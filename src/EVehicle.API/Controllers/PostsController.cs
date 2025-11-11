using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Posts;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý bài đăng
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Posts")]
[AuthorizeRoles("MEMBER")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IAIPriceService _aiPriceService;
    private readonly IValidator<PostCreateRequest> _createValidator;
    private readonly IValidator<PostUpdateRequest> _updateValidator;
    private readonly IValidator<PostCompareRequest> _compareValidator;
    private readonly IValidator<PriceSuggestionRequest> _priceSuggestionValidator;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IPostService postService,
        IAIPriceService aiPriceService,
        IValidator<PostCreateRequest> createValidator,
        IValidator<PostUpdateRequest> updateValidator,
        IValidator<PostCompareRequest> compareValidator,
        IValidator<PriceSuggestionRequest> priceSuggestionValidator,
        ILogger<PostsController> logger)
    {
        _postService = postService;
        _aiPriceService = aiPriceService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _compareValidator = compareValidator;
        _priceSuggestionValidator = priceSuggestionValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC06: Tạo bài đăng mới
    /// </summary>
    /// <param name="categoryId">ID danh mục</param>
    /// <param name="title">Tiêu đề</param>
    /// <param name="description">Mô tả</param>
    /// <param name="price">Giá</param>
    /// <param name="location">Địa điểm</param>
    /// <param name="brand">Thương hiệu</param>
    /// <param name="model">Model</param>
    /// <param name="batteryCapacityCurrent">Dung lượng pin hiện tại</param>
    /// <param name="chargeCount">Số lần sạc</param>
    /// <param name="productionYear">Năm sản xuất</param>
    /// <param name="condition">Tình trạng</param>
    /// <param name="mileage">Số KM (chỉ cho Xe điện)</param>
    /// <param name="packageId">ID gói tin</param>
    /// <param name="images">Danh sách ảnh sản phẩm</param>
    /// <param name="proofImage">Ảnh bằng chứng SOH/KM</param>
    /// <returns>Thông tin bài đăng đã tạo</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<PostResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePost(
        [FromForm] int categoryId,
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] decimal price,
        [FromForm] string location,
        [FromForm] string brand,
        [FromForm] string model,
        [FromForm] decimal batteryCapacityCurrent,
        [FromForm] int? chargeCount,
        [FromForm] int productionYear,
        [FromForm] string condition,
        [FromForm] int? mileage,
        [FromForm] int packageId,
        [FromForm] List<IFormFile> images,
        [FromForm] IFormFile? proofImage,
        [FromForm] bool auctionEnabled = false,
        [FromForm] decimal? startingBid = null,
        [FromForm] decimal? buyNowPrice = null,
        [FromForm] DateTime? auctionEndTime = null)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                // Log để debug
                _logger.LogWarning("Không thể lấy userId từ JWT token. User authenticated: {IsAuthenticated}, Claims count: {ClaimsCount}",
                    User?.Identity?.IsAuthenticated ?? false,
                    User?.Claims?.Count() ?? 0);
                
                // Log tất cả claims để debug
                if (User?.Claims != null)
                {
                    var allClaims = User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                    _logger.LogWarning("All claims in token: {Claims}", string.Join(", ", allClaims));
                }

                return Unauthorized(BaseResponse<PostResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Convert IFormFile to FileUploadDto using helper
            var imageDtos = await FileHelper.ConvertToFileUploadDtoListAsync(images);
            var proofImageDto = await FileHelper.ConvertToFileUploadDtoAsync(proofImage);

            // 3. Create PostCreateRequest từ Application layer (sử dụng DTO từ Application layer)
            var request = new PostCreateRequest
            {
                CategoryId = categoryId,
                Title = title,
                Description = description,
                Price = price,
                Location = location,
                Brand = brand,
                Model = model,
                BatteryCapacityCurrent = batteryCapacityCurrent,
                ChargeCount = chargeCount,
                ProductionYear = productionYear,
                Condition = condition,
                Mileage = mileage,
                PackageId = packageId,
                Images = imageDtos,
                ProofImage = proofImageDto,
                AuctionEnabled = auctionEnabled,
                StartingBid = startingBid,
                BuyNowPrice = buyNowPrice,
                AuctionEndTime = auctionEndTime
            };

            // 4. Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PostResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 5. Call service
            var response = await _postService.CreatePostAsync(userId.Value, request);

            // 6. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetPostById),
                    new { id = response.Data?.PostId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bài đăng");
            return StatusCode(500, BaseResponse<PostResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo bài đăng"));
        }
    }

    /// <summary>
    /// UC15: Tìm kiếm Sản phẩm
    /// Tìm kiếm danh sách bài đăng đã được duyệt (cho Member/Public)
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang:
    /// - Keyword: Từ khóa tìm kiếm (tìm trong title, description)
    /// - CategoryId: ID danh mục
    /// - Brand: Thương hiệu
    /// - Model: Model
    /// - Location: Địa điểm
    /// - MinPrice/MaxPrice: Khoảng giá
    /// - MinProductionYear/MaxProductionYear: Khoảng năm sản xuất
    /// - MinBatteryCapacity/MaxBatteryCapacity: Khoảng dung lượng pin (SOH)
    /// - MinMileage/MaxMileage: Khoảng số km (chỉ cho Xe điện)
    /// - Condition: Tình trạng
    /// - AuctionOnly: Chỉ hiển thị bài đăng đang đấu giá
    /// - SortBy: Sắp xếp theo (price, approvedAt, popularity/priorityLevel)
    /// - SortDirection: Hướng sắp xếp (asc, desc)
    /// </param>
    /// <returns>Danh sách bài đăng</returns>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchPosts([FromQuery] PostSearchRequest request)
    {
        try
        {
            var response = await _postService.SearchApprovedPostsAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tìm kiếm bài đăng");
            return StatusCode(500, PagedResponse<PostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi tìm kiếm bài đăng"));
        }
    }

    /// <summary>
    /// Lấy thông tin bài đăng theo ID (chi tiết - Public view, ẩn thông tin nhạy cảm)
    /// </summary>
    /// <param name="id">ID bài đăng</param>
    /// <returns>Thông tin bài đăng chi tiết (ẩn thông tin người bán, admin, staff, gói tin)</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<PostPublicDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        try
        {
            // Kiểm tra xem user có đăng nhập không
            var userId = ClaimsHelper.GetUserId(User);
            var userRole = ClaimsHelper.GetRole(User);

            // Nếu user đã đăng nhập và là ADMIN hoặc STAFF, trả về full detail
            if (userId.HasValue && (userRole == "ADMIN" || userRole == "STAFF"))
            {
                var response = await _postService.GetPostByIdAsync(id);
                if (response.Success)
                {
                    return Ok(response);
                }
                return NotFound(response);
            }

            // Nếu MEMBER đã đăng nhập, kiểm tra xem có phải bài đăng của họ không
            if (userId.HasValue && userRole == "MEMBER")
            {
                // Lấy full detail để kiểm tra ownership
                var fullDetailResponse = await _postService.GetPostByIdAsync(id);
                if (fullDetailResponse.Success && fullDetailResponse.Data != null)
                {
                    // Kiểm tra ownership
                    if (fullDetailResponse.Data.UserId == userId.Value)
                    {
                        // User là chủ sở hữu, trả về full detail
                        return Ok(fullDetailResponse);
                    }
                }
                // Nếu không phải chủ sở hữu hoặc không tìm thấy, fallback về public view
            }

            // Nếu là guest hoặc MEMBER không phải chủ sở hữu, trả về public view (ẩn thông tin nhạy cảm)
            var publicResponse = await _postService.GetPostByIdPublicAsync(id);
            if (publicResponse.Success)
            {
                return Ok(publicResponse);
            }

            return NotFound(publicResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin bài đăng, PostId: {PostId}", id);
            return StatusCode(500, BaseResponse<PostPublicDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy thông tin bài đăng"));
        }
    }

    /// <summary>
    /// UC07: Chỉnh sửa bài đăng
    /// </summary>
    /// <param name="id">ID bài đăng</param>
    /// <param name="request">Thông tin cập nhật (JSON body, files sẽ được xử lý riêng nếu cần)</param>
    /// <returns>Thông tin bài đăng đã cập nhật</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePost(
        Guid id,
        [FromBody] PostUpdateRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<PostResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PostResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _postService.UpdatePostAsync(userId.Value, id, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật bài đăng, PostId: {PostId}", id);
            return StatusCode(500, BaseResponse<PostResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật bài đăng"));
        }
    }

    /// <summary>
    /// UC08: Xóa bài đăng
    /// </summary>
    /// <param name="id">ID bài đăng</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _postService.DeletePostAsync(userId.Value, id);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bài đăng, PostId: {PostId}", id);
            return StatusCode(500, BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi xóa bài đăng"));
        }
    }

    /// <summary>
    /// UC09: Tạm ẩn/Hiện bài đăng
    /// </summary>
    /// <param name="id">ID bài đăng</param>
    /// <param name="request">Trạng thái is_active mới</param>
    /// <returns>Thông tin bài đăng đã cập nhật</returns>
    [HttpPatch("{id:guid}/toggle-active")]
    [ProducesResponseType(typeof(BaseResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TogglePostActive(
        Guid id,
        [FromBody] PostToggleActiveRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<PostResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _postService.TogglePostActiveAsync(userId.Value, id, request);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thay đổi trạng thái bài đăng, PostId: {PostId}", id);
            return StatusCode(500, BaseResponse<PostResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi thay đổi trạng thái bài đăng"));
        }
    }

    /// <summary>
    /// UC13: Xem danh sách bài đăng của mình
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách bài đăng của user</returns>
    [HttpGet("my-posts")]
    [ProducesResponseType(typeof(PagedResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyPosts([FromQuery] MyPostsSearchRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(PagedResponse<PostResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _postService.GetMyPostsAsync(userId.Value, request);

            // 3. Return response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài đăng của user");
            return StatusCode(500, PagedResponse<PostResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài đăng"));
        }
    }

    /// <summary>
    /// UC20: So sánh Sản phẩm
    /// Người dùng so sánh nhiều sản phẩm với nhau (tối đa 3-5 sản phẩm)
    /// </summary>
    /// <param name="request">Danh sách ID bài đăng cần so sánh (2-5 sản phẩm)</param>
    /// <returns>Bảng so sánh các thông số: Giá, SOH, Số km, Năm sản xuất, Hãng, v.v.</returns>
    [HttpPost("compare")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<PostCompareResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ComparePosts([FromBody] PostCompareRequest request)
    {
        try
        {
            // 1. Validate request
            var validationResult = await _compareValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PostCompareResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 2. Call service
            var response = await _postService.ComparePostsAsync(request);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi so sánh sản phẩm");
            return StatusCode(500, BaseResponse<PostCompareResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi so sánh sản phẩm"));
        }
    }

    /// <summary>
    /// UC51: Gợi ý Giá bán
    /// Hệ thống AI gợi ý giá bán phù hợp cho bài đăng sử dụng Google Gemini
    /// </summary>
    /// <param name="request">Thông tin sản phẩm để AI phân tích và gợi ý giá</param>
    /// <returns>Giá gợi ý, độ tin cậy và các yếu tố ảnh hưởng</returns>
    [HttpPost("suggest-price")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<PriceSuggestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SuggestPrice([FromBody] PriceSuggestionRequest request)
    {
        try
        {
            // 1. Validate request
            var validationResult = await _priceSuggestionValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PriceSuggestionResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 2. Call AI service
            var response = await _aiPriceService.SuggestPriceAsync(request);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gợi ý giá từ AI");
            return StatusCode(500, BaseResponse<PriceSuggestionResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo gợi ý giá"));
        }
    }
}

