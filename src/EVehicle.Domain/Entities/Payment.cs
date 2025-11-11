using EVehicle.Domain.Common;

namespace EVehicle.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid PaymentId => Id;
    public Guid UserId { get; set; }
    public Guid? UserCreditId { get; set; } // Cho thanh toán gói tin (PACKAGE)
    public Guid? OrderId { get; set; } // Cho thanh toán giao dịch mua bán (TRANSACTION)
    public int? PackageId { get; set; } // Gói tin đã mua (nếu payment_type = 'PACKAGE')
    public decimal Amount { get; set; }
    public string PaymentGateway { get; set; } = string.Empty; // PAYOS
    public string? TransactionCode { get; set; } // Mã giao dịch từ cổng TT
    public string Status { get; set; } = "PENDING"; // PENDING, SUCCESS, FAILED
    public string PaymentType { get; set; } = string.Empty; // PACKAGE, TRANSACTION
    public DateTime? CompletedAt { get; set; } // Thời gian hoàn tất thanh toán

    // Navigation properties
    public User User { get; set; } = null!;
    public UserPackageCredits? UserPackageCredits { get; set; }
    public Order? Order { get; set; }
    public PackageDefinition? Package { get; set; }
}

