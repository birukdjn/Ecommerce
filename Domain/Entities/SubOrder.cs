using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class SubOrder:BaseEntity
    {
        public Guid OrderId { get; set; }
        public virtual Order MasterOrder { get; set; } = null!;

        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }= null!;

        public decimal SubTotal { get; set; }
        public decimal PlatformCommission { get; set; }

        public SubOrderStatus Status { get; set; } = SubOrderStatus.Pending;
        public virtual ICollection<OrderItem> Items { get; set; } = [];

    }
}
