using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? RelatedOrderId { get; set; }

        public virtual VendorWallet Wallet { get; set; } = null!;
    }
}
