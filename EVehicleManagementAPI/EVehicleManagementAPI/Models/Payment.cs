using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVehicleManagementAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        // --- Buyer ---
        public int BuyerId { get; set; }

        // --- Seller ---
        public int SellerId { get; set; }

        public decimal Amount { get; set; }
        public string Method { get; set; } // Banking, COD, ShopeePay, ...
        public string TransferContent { get; set; } // Noi dung chuyen khoan
        public string Status { get; set; } // Pending, Completed, Refunded, Failed
        public DateTime CreatedAt { get; set; }

        // --- Navigation properties ---
        [ForeignKey(nameof(BuyerId))]
        [InverseProperty("PaymentsAsBuyer")]
        public Member Buyer { get; set; }

        [ForeignKey(nameof(SellerId))]
        [InverseProperty("PaymentsAsSeller")]
        public Member Seller { get; set; }

        //lien quan den giao dich
        public ICollection<Construct> Constructs { get; set; } = new List<Construct>();
        public ICollection<PostPackageSub> PostPackageSubs { get; set; } = new List<PostPackageSub>();
    }
}
