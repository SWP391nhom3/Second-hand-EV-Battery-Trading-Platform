using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Leads;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVehicle.Application.Validators.Leads;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý Lead
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Leads")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;
    private readonly IValidator<LeadCreateRequest> _createValidator;
    private readonly IValidator<LeadStatusUpdateRequest> _statusUpdateValidator;
    private readonly IValidator<LeadAssignStaffRequest> _assignStaffValidator;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(
        ILeadService leadService,
        IValidator<LeadCreateRequest> createValidator,
        IValidator<LeadStatusUpdateRequest> statusUpdateValidator,
        IValidator<LeadAssignStaffRequest> assignStaffValidator,
        ILogger<LeadsController> logger)
    {
        _leadService = leadService;
        _createValidator = createValidator;
        _statusUpdateValidator = statusUpdateValidator;
        _assignStaffValidator = assignStaffValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC23: Tạo Lead (Đặt lịch xem / Yêu cầu Môi giới)
    /// </summary>
    /// <param name="request">Thông tin Lead</param>
    /// <returns>Thông tin Lead đã tạo</returns>
    [HttpPost]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<LeadResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLead([FromBody] LeadCreateRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<LeadResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<LeadResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _leadService.CreateLeadAsync(userId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(CreateLead),
                    new { id = response.Data?.LeadId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo Lead");
            return StatusCode(500, BaseResponse<LeadResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo Lead"));
        }
    }

    /// <summary>
    /// UC46: Lấy danh sách tất cả Leads (Admin only)
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách Leads</returns>
    [HttpGet("all")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(PagedResponse<LeadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllLeads([FromQuery] LeadSearchRequest request)
    {
        try
        {
            // 1. Validate and normalize request (PagedRequest has built-in validation)
            request.IsValid();

            // 2. Call service
            var response = await _leadService.GetAllLeadsAsync(request);

            // 3. Return response
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách tất cả Leads");
            return StatusCode(500, PagedResponse<LeadResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách Leads"));
        }
    }

    /// <summary>
    /// UC23: Lấy danh sách Leads của Member (người mua)
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách Leads</returns>
    [HttpGet("my-leads")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<PagedResponse<LeadResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyLeads([FromQuery] LeadSearchRequest request)
    {
        try
        {
            // 1. Get buyerId from JWT token
            var buyerId = ClaimsHelper.GetUserId(User);
            if (buyerId == null)
            {
                return Unauthorized(BaseResponse<PagedResponse<LeadResponse>>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _leadService.GetMyLeadsAsync(buyerId.Value, request);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách Leads của Member");
            return StatusCode(500, BaseResponse<PagedResponse<LeadResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách Leads"));
        }
    }

    /// <summary>
    /// UC40: Lấy danh sách Lead được gán cho Staff
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách Leads</returns>
    [HttpGet("staff")]
    [AuthorizeRoles("STAFF")]
    [ProducesResponseType(typeof(BaseResponse<PagedResponse<LeadResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLeadsByStaff([FromQuery] LeadSearchRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<PagedResponse<LeadResponse>>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _leadService.GetLeadsByStaffIdAsync(staffId.Value, request);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách Leads");
            return StatusCode(500, BaseResponse<PagedResponse<LeadResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách Leads"));
        }
    }

    /// <summary>
    /// Lấy chi tiết Lead
    /// </summary>
    /// <param name="id">ID Lead</param>
    /// <returns>Chi tiết Lead</returns>
    [HttpGet("{id}")]
    [AuthorizeRoles("STAFF", "MEMBER", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<LeadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLeadById(Guid id)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<LeadResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Get user role
            var userRole = ClaimsHelper.GetRole(User);
            
            // 3. If user is STAFF, pass staffId to validate ownership
            Guid? staffId = userRole == "STAFF" ? userId : null;

            // 4. Call service
            var response = await _leadService.GetLeadByIdAsync(id, staffId);

            // 5. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết Lead");
            return StatusCode(500, BaseResponse<LeadResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết Lead"));
        }
    }

    /// <summary>
    /// UC44: Cập nhật trạng thái Lead
    /// </summary>
    /// <param name="id">ID Lead</param>
    /// <param name="request">Thông tin cập nhật trạng thái</param>
    /// <returns>Lead đã được cập nhật</returns>
    [HttpPut("{id}/status")]
    [AuthorizeRoles("STAFF")]
    [ProducesResponseType(typeof(BaseResponse<LeadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLeadStatus(Guid id, [FromBody] LeadStatusUpdateRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<LeadResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _statusUpdateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<LeadResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _leadService.UpdateLeadStatusAsync(staffId.Value, id, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái Lead");
            return StatusCode(500, BaseResponse<LeadResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật trạng thái Lead"));
        }
    }

    /// <summary>
    /// UC46: Admin gán Staff cho Lead
    /// </summary>
    /// <param name="id">ID của Lead</param>
    /// <param name="request">Thông tin Staff được gán</param>
    /// <returns>Thông tin Lead đã được cập nhật</returns>
    [HttpPost("{id}/assign-staff")]
    [AuthorizeRoles("ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<LeadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignStaffToLead(Guid id, [FromBody] LeadAssignStaffRequest request)
    {
        try
        {
            // 1. Get adminId from JWT token
            var adminId = ClaimsHelper.GetUserId(User);
            if (adminId == null)
            {
                return Unauthorized(BaseResponse<LeadResponse>.FailureResponse(
                    "Không thể xác định người dùng"));
            }

            // 2. Validate request
            var validationResult = await _assignStaffValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<LeadResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _leadService.AssignStaffToLeadAsync(adminId.Value, id, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gán Staff cho Lead, LeadId: {LeadId}", id);
            return StatusCode(500, BaseResponse<LeadResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi gán Staff cho Lead"));
        }
    }
}

