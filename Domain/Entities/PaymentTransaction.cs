

using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class PaymentTransaction:BaseEntity
    {
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public required string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? VerifiedAt { get; set; }
    }
}
