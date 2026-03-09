
using Domain.Common;

namespace Domain.Entities
{
    public class VendorWallet : VersionedEntity
    {
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        public decimal Balance { get; set; } = 0;
        public decimal EscrowBalance { get; set; } = 0;
    }
}
