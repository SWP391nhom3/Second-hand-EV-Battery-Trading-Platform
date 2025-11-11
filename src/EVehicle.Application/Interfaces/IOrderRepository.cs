using EVehicle.Domain.Entities;

namespace EVehicle.Application.Interfaces;

/// <summary>
/// Repository interface cho Order entity
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Lấy Order theo ID
    /// </summary>
    Task<Order?> GetByIdAsync(Guid orderId);

    /// <summary>
    /// Lấy Order theo ID với đầy đủ thông tin
    /// </summary>
    Task<Order?> GetByIdWithDetailsAsync(Guid orderId);

    /// <summary>
    /// Lấy Order theo LeadId
    /// </summary>
    Task<Order?> GetByLeadIdAsync(Guid leadId);

    /// <summary>
    /// Lấy danh sách Order của một Buyer
    /// </summary>
    Task<List<Order>> GetOrdersByBuyerIdAsync(Guid buyerId);

    /// <summary>
    /// Lấy danh sách Order của một Seller
    /// </summary>
    Task<List<Order>> GetOrdersBySellerIdAsync(Guid sellerId);

    /// <summary>
    /// Tạo Order mới
    /// </summary>
    Task<Order> CreateAsync(Order order);

    /// <summary>
    /// Cập nhật Order
    /// </summary>
    Task<Order> UpdateAsync(Order order);

    /// <summary>
    /// Lưu changes
    /// </summary>
    Task SaveChangesAsync();
}

