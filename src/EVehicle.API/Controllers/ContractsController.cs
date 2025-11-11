using EVehicle.API.Attributes;
using EVehicle.API.Helpers;
using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Contracts;
using EVehicle.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVehicle.API.Controllers;

/// <summary>
/// Controller xử lý các API quản lý hợp đồng
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Contracts")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly IValidator<ContractCreateRequest> _contractCreateValidator;
    private readonly IValidator<ContractSignRequest> _contractSignValidator;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        IContractService contractService,
        IValidator<ContractCreateRequest> contractCreateValidator,
        IValidator<ContractSignRequest> contractSignValidator,
        ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _contractCreateValidator = contractCreateValidator;
        _contractSignValidator = contractSignValidator;
        _logger = logger;
    }

    /// <summary>
    /// UC43: Lấy danh sách mẫu hợp đồng
    /// </summary>
    /// <param name="categoryId">ID danh mục (optional)</param>
    /// <returns>Danh sách mẫu hợp đồng</returns>
    [HttpGet("templates")]
    [AuthorizeRoles("STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<List<ContractTemplateResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetContractTemplates([FromQuery] int? categoryId = null)
    {
        try
        {
            var response = await _contractService.GetContractTemplatesAsync(categoryId);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách mẫu hợp đồng");
            return StatusCode(500, BaseResponse<List<ContractTemplateResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách mẫu hợp đồng"));
        }
    }

    /// <summary>
    /// UC43: Staff tạo hợp đồng từ mẫu
    /// </summary>
    /// <param name="request">Thông tin hợp đồng</param>
    /// <returns>Hợp đồng đã tạo</returns>
    [HttpPost]
    [AuthorizeRoles("STAFF")]
    [ProducesResponseType(typeof(BaseResponse<ContractResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateContract([FromBody] ContractCreateRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<ContractResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _contractCreateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<ContractResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _contractService.CreateContractAsync(staffId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetContractById),
                    new { id = response.Data?.ContractId },
                    response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo hợp đồng");
            return StatusCode(500, BaseResponse<ContractResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi tạo hợp đồng"));
        }
    }

    /// <summary>
    /// Lấy chi tiết hợp đồng
    /// </summary>
    /// <param name="id">ID hợp đồng</param>
    /// <returns>Chi tiết hợp đồng</returns>
    [HttpGet("{id}")]
    [AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetContractById(Guid id)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<ContractResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _contractService.GetContractByIdAsync(id, userId);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết hợp đồng");
            return StatusCode(500, BaseResponse<ContractResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy chi tiết hợp đồng"));
        }
    }

    /// <summary>
    /// UC29: Người mua/người bán ký hợp đồng
    /// </summary>
    /// <param name="id">ID hợp đồng</param>
    /// <param name="request">Thông tin chữ ký</param>
    /// <returns>Hợp đồng đã được cập nhật</returns>
    [HttpPost("{id}/sign")]
    [AuthorizeRoles("MEMBER")]
    [ProducesResponseType(typeof(BaseResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignContract(Guid id, [FromBody] ContractSignRequest request)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<ContractResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Validate request
            var validationResult = await _contractSignValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(BaseResponse<ContractResponse>.FailureResponse(
                    "Dữ liệu không hợp lệ",
                    errors));
            }

            // 3. Call service
            var response = await _contractService.SignContractAsync(userId.Value, id, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ký hợp đồng");
            return StatusCode(500, BaseResponse<ContractResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi ký hợp đồng"));
        }
    }

    /// <summary>
    /// Tải xuống file PDF hợp đồng
    /// </summary>
    /// <param name="id">ID hợp đồng</param>
    /// <returns>URL file PDF</returns>
    [HttpGet("{id}/pdf")]
    [AuthorizeRoles("MEMBER", "STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetContractPdfUrl(Guid id)
    {
        try
        {
            // 1. Get userId from JWT token
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(BaseResponse<string>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _contractService.GetContractPdfUrlAsync(id, userId);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy URL PDF hợp đồng");
            return StatusCode(500, BaseResponse<string>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy URL PDF hợp đồng"));
        }
    }

    /// <summary>
    /// UC43: Lấy danh sách hợp đồng của Staff
    /// </summary>
    /// <param name="request">Thông tin tìm kiếm và phân trang</param>
    /// <returns>Danh sách hợp đồng</returns>
    [HttpGet("staff")]
    [AuthorizeRoles("STAFF", "ADMIN")]
    [ProducesResponseType(typeof(BaseResponse<PagedResponse<ContractResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStaffContracts([FromQuery] ContractSearchRequest request)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<PagedResponse<ContractResponse>>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Set staffId from token (security)
            request.StaffId = staffId.Value;

            // 3. Call service
            var response = await _contractService.GetContractsByStaffIdAsync(staffId.Value, request);

            // 4. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách hợp đồng của Staff");
            return StatusCode(500, BaseResponse<PagedResponse<ContractResponse>>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi lấy danh sách hợp đồng"));
        }
    }

    /// <summary>
    /// UC43: Gửi hợp đồng để ký
    /// </summary>
    /// <param name="id">ID hợp đồng</param>
    /// <returns>Hợp đồng đã được cập nhật</returns>
    [HttpPost("{id}/send-for-signature")]
    [AuthorizeRoles("STAFF")]
    [ProducesResponseType(typeof(BaseResponse<ContractResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendContractForSignature(Guid id)
    {
        try
        {
            // 1. Get staffId from JWT token
            var staffId = ClaimsHelper.GetUserId(User);
            if (staffId == null)
            {
                return Unauthorized(BaseResponse<ContractResponse>.FailureResponse(
                    "Không thể xác định người dùng. Vui lòng đăng nhập lại."));
            }

            // 2. Call service
            var response = await _contractService.SendContractForSignatureAsync(staffId.Value, id);

            // 3. Return response
            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi hợp đồng để ký");
            return StatusCode(500, BaseResponse<ContractResponse>.FailureResponse(
                ex,
                "Đã xảy ra lỗi khi gửi hợp đồng để ký"));
        }
    }
}

