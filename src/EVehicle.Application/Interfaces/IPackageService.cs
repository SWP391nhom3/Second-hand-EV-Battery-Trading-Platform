using EVehicle.Application.DTOs.Common;
using EVehicle.Application.DTOs.Packages;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Interface cho Package Service
/// </summary>
public interface IPackageService
{
    Task<BaseResponse<List<PackageResponse>>> GetPackagesWithCreditsAsync(Guid? userId);
    
    /// <summary>
    /// UC27: Lấy danh sách gói tin mà user đã mua cùng với số credits còn lại
    /// </summary>
    Task<BaseResponse<List<UserPackageCreditsResponse>>> GetUserPackagesWithCreditsAsync(Guid userId);

    /// <summary>
    /// UC26: Mua gói tin
    /// </summary>
    Task<BaseResponse<PackagePurchaseResponse>> PurchasePackageAsync(
        Guid userId,
        PackagePurchaseRequest request);

    /// <summary>
    /// UC48: Lấy danh sách gói tin với phân trang (Admin)
    /// </summary>
    Task<PagedResponse<PackageDetailResponse>> GetPackagesAsync(PackageSearchRequest request);

    /// <summary>
    /// UC48: Lấy chi tiết gói tin theo ID (Admin)
    /// </summary>
    Task<BaseResponse<PackageDetailResponse>> GetPackageByIdAsync(int packageId);

    /// <summary>
    /// UC48: Tạo gói tin mới (Admin)
    /// </summary>
    Task<BaseResponse<PackageDetailResponse>> CreatePackageAsync(PackageCreateRequest request);

    /// <summary>
    /// UC48: Cập nhật gói tin (Admin)
    /// </summary>
    Task<BaseResponse<PackageDetailResponse>> UpdatePackageAsync(int packageId, PackageUpdateRequest request);

    /// <summary>
    /// UC48: Kích hoạt/vô hiệu hóa gói tin (Admin)
    /// </summary>
    Task<BaseResponse> TogglePackageStatusAsync(int packageId);
}

