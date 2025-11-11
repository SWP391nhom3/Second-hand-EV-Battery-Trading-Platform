using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Contracts;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Service interface cho Contract operations
/// </summary>
public interface IContractService
{
    /// <summary>
    /// UC43: Lấy danh sách mẫu hợp đồng
    /// </summary>
    Task<BaseResponse<List<ContractTemplateResponse>>> GetContractTemplatesAsync(int? categoryId = null);

    /// <summary>
    /// UC43: Staff tạo hợp đồng từ mẫu
    /// </summary>
    Task<BaseResponse<ContractResponse>> CreateContractAsync(
        Guid staffId,
        ContractCreateRequest request);

    /// <summary>
    /// Lấy chi tiết hợp đồng
    /// </summary>
    Task<BaseResponse<ContractResponse>> GetContractByIdAsync(Guid contractId, Guid? userId = null);

    /// <summary>
    /// UC29: Người mua/người bán ký hợp đồng
    /// </summary>
    Task<BaseResponse<ContractResponse>> SignContractAsync(
        Guid userId,
        Guid contractId,
        ContractSignRequest request);

    /// <summary>
    /// Tải xuống file PDF hợp đồng
    /// </summary>
    Task<BaseResponse<string>> GetContractPdfUrlAsync(Guid contractId, Guid? userId = null);

    /// <summary>
    /// UC43: Lấy danh sách hợp đồng của Staff
    /// </summary>
    Task<BaseResponse<PagedResponse<ContractResponse>>> GetContractsByStaffIdAsync(
        Guid staffId,
        ContractSearchRequest request);

    /// <summary>
    /// UC43: Gửi hợp đồng để ký
    /// </summary>
    Task<BaseResponse<ContractResponse>> SendContractForSignatureAsync(
        Guid staffId,
        Guid contractId);
}

