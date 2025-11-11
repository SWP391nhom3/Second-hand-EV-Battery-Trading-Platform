using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Appointments;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý Appointment (Lịch hẹn)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Appointments")]
[AuthorizeRoles("STAFF")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IValidator<AppointmentCreateRequest> _createValidator;
    private readonly IValidator<AppointmentUpdateRequest> _updateValidator;
    private readonly IValidator<AppointmentStatusUpdateRequest> _statusUpdateValidator;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IValidator<AppointmentCreateRequest> createValidator,
        IValidator<AppointmentUpdateRequest> updateValidator,
        IValidator<AppointmentStatusUpdateRequest> statusUpdateValidator,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusUpdateValidator = statusUpdateValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC41: Tạo Appointment (Tạo Lịch hẹn)
    /// </summary>
    /// <param name="request">Thông tin Appointment</param>
    /// <returns>Thông tin Appointment đã tạo</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<AppointmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<AppointmentResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _appointmentService.CreateAppointmentAsync(staffId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetAppointmentById),
                    new { id = response.Data?.AppointmentId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo Appointment");
            return StatusCode(500, BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo Appointment"));
        }
    }

    /// <summary>
    /// UC42: Lấy danh sách Appointments (Quản lý Lịch hẹn)
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách Appointments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<PagedResponse<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppointments([FromQuery] AppointmentSearchRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<PagedResponse<AppointmentResponse>>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _appointmentService.GetAppointmentsAsync(staffId.Value, request);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách Appointments");
            return StatusCode(500, BaseResponse<PagedResponse<AppointmentResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách Appointments"));
        }
    }

    /// <summary>
    /// UC42: Lấy chi tiết Appointment
    /// </summary>
    /// <param name="id">ID Appointment</param>
    /// <returns>Chi tiết Appointment</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _appointmentService.GetAppointmentByIdAsync(id, staffId.Value);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết Appointment");
            return StatusCode(500, BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết Appointment"));
        }
    }

    /// <summary>
    /// UC42: Cập nhật Appointment
    /// </summary>
    /// <param name="id">ID Appointment</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Thông tin Appointment đã cập nhật</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BaseResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAppointment(
        Guid id,
        [FromBody] AppointmentUpdateRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<AppointmentResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _appointmentService.UpdateAppointmentAsync(id, staffId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật Appointment");
            return StatusCode(500, BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật Appointment"));
        }
    }

    /// <summary>
    /// UC42: Hủy Appointment
    /// </summary>
    /// <param name="id">ID Appointment</param>
    /// <returns>Thông tin Appointment đã hủy</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(BaseResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelAppointment(Guid id)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _appointmentService.CancelAppointmentAsync(id, staffId.Value);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi hủy Appointment");
            return StatusCode(500, BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi hủy Appointment"));
        }
    }

    /// <summary>
    /// UC42: Cập nhật trạng thái Appointment
    /// </summary>
    /// <param name="id">ID Appointment</param>
    /// <param name="request">Thông tin cập nhật trạng thái</param>
    /// <returns>Thông tin Appointment đã cập nhật</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(BaseResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAppointmentStatus(
        Guid id,
        [FromBody] AppointmentStatusUpdateRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<AppointmentResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _statusUpdateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<AppointmentResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _appointmentService.UpdateAppointmentStatusAsync(id, staffId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái Appointment");
            return StatusCode(500, BaseResponse<AppointmentResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi cập nhật trạng thái Appointment"));
        }
    }
}

