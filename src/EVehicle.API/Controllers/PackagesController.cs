using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Packages;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý gói tin
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Packages")]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _packageService;
    private readonly IValidator<PackagePurchaseRequest> _purchaseValidator;
    private readonly IValidator<PackageCreateRequest> _createValidator;
    private readonly IValidator<PackageUpdateRequest> _updateValidator;
    private readonly ILogger<PackagesController> _logger;

    public PackagesController(
        IPackageService packageService,
        IValidator<PackagePurchaseRequest> purchaseValidator,
        IValidator<PackageCreateRequest> createValidator,
        IValidator<PackageUpdateRequest> updateValidator,
        ILogger<PackagesController> logger)
    {
        _packageService = packageService;
        _purchaseValidator = purchaseValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả gói tin với credits còn lại của user (dùng cho select2)
    /// </summary>
    /// <returns>Danh sách gói tin</returns>
    /// <remarks>
    /// - Nếu user đã đăng nhập: Hiển thị số credits còn lại cho mỗi gói
    /// - Nếu user chưa đăng nhập: Chỉ hiển thị thông tin gói tin
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<List<PackageResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPackagesWithCredits()
    {
        try
        {
            // Lấy userId từ token (nếu có)
            Guid? userId = ClaimsHelper.GetUserId(User);

            var response = await _packageService.GetPackagesWithCreditsAsync(userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách gói tin");
            return StatusCode(500, BaseResponse<List<PackageResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách gói tin"));
        }
    }

    /// <summary>
    /// UC27: Lấy danh sách gói tin mà user đã mua cùng với số credits còn lại
    /// </summary>
    /// <returns>Danh sách gói tin của user với credits còn lại</returns>
    [HttpGet("my-packages")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<List<UserPackageCreditsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyPackagesWithCredits()
    {
        try
        {
            // Lấy userId từ token
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

                return Unauthorized(BaseResponse<List<UserPackageCreditsResponse>>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            var response = await _packageService.GetUserPackagesWithCreditsAsync(userId.Value);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách gói tin của user");
            return StatusCode(500, BaseResponse<List<UserPackageCreditsResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách gói tin của user"));
        }
    }

    /// <summary>
    /// UC26: Mua gói tin
    /// </summary>
    /// <param name="request">Thông tin mua gói tin</param>
    /// <returns>Thông tin thanh toán</returns>
    [HttpPost("purchase")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<PackagePurchaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PurchasePackage([FromBody] PackagePurchaseRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<PackagePurchaseResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _purchaseValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PackagePurchaseResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _packageService.PurchasePackageAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi mua gói tin");
            return StatusCode(500, BaseResponse<PackagePurchaseResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi mua gói tin"));
        }
    }

    #region UC48: Admin Quản lý Gói tin

    /// <summary>
    /// UC48: Lấy danh sách gói tin với phân trang (Admin)
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách gói tin</returns>
    [HttpGet("admin")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(PagedResponse<PackageDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPackages([FromQuery] PackageSearchRequest request)
    {
        try
        {
            var response = await _packageService.GetPackagesAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách gói tin");
            return StatusCode(500, PagedResponse<PackageDetailResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách gói tin"));
        }
    }

    /// <summary>
    /// UC48: Lấy chi tiết gói tin theo ID (Admin)
    /// </summary>
    /// <param name="id">ID gói tin</param>
    /// <returns>Chi tiết gói tin</returns>
    [HttpGet("admin/{id}")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<PackageDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPackageById(int id)
    {
        try
        {
            var response = await _packageService.GetPackageByIdAsync(id);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết gói tin, PackageId: {PackageId}", id);
            return StatusCode(500, BaseResponse<PackageDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết gói tin"));
        }
    }

    /// <summary>
    /// UC48: Tạo gói tin mới (Admin)
    /// </summary>
    /// <param name="request">Thông tin gói tin mới</param>
    /// <returns>Thông tin gói tin đã tạo</returns>
    [HttpPost("admin")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<PackageDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePackage([FromBody] PackageCreateRequest request)
    {
        try
        {
            // 1. Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PackageDetailResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 2. Call service
            var response = await _packageService.CreatePackageAsync(request);

            // 3. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetPackageById),
                    new { id = response.Data?.Id },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo gói tin");
            return StatusCode(500, BaseResponse<PackageDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo gói tin"));
        }
    }

    /// <summary>
    /// UC48: Cập nhật gói tin (Admin)
    /// </summary>
    /// <param name="id">ID gói tin</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Thông tin gói tin đã cập nhật</returns>
    [HttpPut("admin/{id}")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<PackageDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePackage(int id, [FromBody] PackageUpdateRequest request)
    {
        try
        {
            // 1. Validate request
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<PackageDetailResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 2. Call service
            var response = await _packageService.UpdatePackageAsync(id, request);

            // 3. Return response
            if (!response.Success)
            {
                if (response.Message.Contains("Không tìm thấy"))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật gói tin, PackageId: {PackageId}", id);
            return StatusCode(500, BaseResponse<PackageDetailResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật gói tin"));
        }
    }

    /// <summary>
    /// UC48: Kích hoạt/vô hiệu hóa gói tin (Admin)
    /// </summary>
    /// <param name="id">ID gói tin</param>
    /// <returns>Kết quả thao tác</returns>
    [HttpPatch("admin/{id}/toggle-status")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TogglePackageStatus(int id)
    {
        try
        {
            var response = await _packageService.TogglePackageStatusAsync(id);
            if (!response.Success)
            {
                if (response.Message.Contains("Không tìm thấy"))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thay đổi trạng thái gói tin, PackageId: {PackageId}", id);
            return StatusCode(500, BaseResponse.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi thay đổi trạng thái gói tin"));
        }
    }

    #endregion
}

