
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class PayoutRequest:BaseEntity
    {
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        public decimal Amount { get; set; }
        public PayoutRequestStatus Status { get; set; } = PayoutRequestStatus.Pending;
        public DateTime? ProcessedAt { get; set; }
    }
}
