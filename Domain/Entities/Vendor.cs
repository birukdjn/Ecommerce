
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Vendor:BaseEntity
    {
        public required string StoreName { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? LicenseUrl { get; set; }
        public VendorStatus Status { get; set; } = VendorStatus.Pending;
        public string? RejectionReason { get; set; }

        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual VendorWallet? Wallet { get; set; }
        public virtual ICollection<Product> Products { get; set; } = [];
        public virtual ICollection<SubOrder> SubOrders { get; set; } = [];
    }
}
