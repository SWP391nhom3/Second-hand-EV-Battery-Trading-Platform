using EVehicle.Application.DTOs.Common;
using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho Payment entity
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Lấy Payment theo ID
    /// </summary>
    Task<Payment?> GetByIdAsync(Guid paymentId);

    /// <summary>
    /// Lấy Payment theo ID với đầy đủ thông tin (cho chi tiết)
    /// </summary>
    Task<Payment?> GetByIdWithDetailsAsync(Guid paymentId);

    /// <summary>
    /// Lấy Payment theo TransactionCode
    /// </summary>
    Task<Payment?> GetByTransactionCodeAsync(string transactionCode);

    /// <summary>
    /// Lấy Payment theo PaymentId (để xử lý webhook khi TransactionCode chưa có)
    /// </summary>
    Task<Payment?> GetByIdForUpdateAsync(Guid paymentId);

    /// <summary>
    /// Lấy Payment theo OrderId
    /// </summary>
    Task<Payment?> GetByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Lấy Payment theo PackageId và UserId (cho thanh toán gói tin)
    /// </summary>
    Task<Payment?> GetByPackageIdAndUserIdAsync(int packageId, Guid userId);

    /// <summary>
    /// Lấy danh sách Payment của một User
    /// </summary>
    Task<List<Payment>> GetPaymentsByUserIdAsync(Guid userId);

    /// <summary>
    /// UC30: Tìm kiếm lịch sử thanh toán với filter và phân trang
    /// </summary>
    Task<PagedResult<Payment>> SearchPaymentsAsync(
        Guid userId,
        string? paymentType,
        string? status,
        string? paymentGateway,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string? sortDirection);

    /// <summary>
    /// Tạo Payment mới
    /// </summary>
    Task<Payment> CreateAsync(Payment payment);

    /// <summary>
    /// Cập nhật Payment
    /// </summary>
    Task<Payment> UpdateAsync(Payment payment);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}

