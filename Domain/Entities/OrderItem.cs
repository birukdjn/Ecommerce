
using Domain.Common;

namespace Domain.Entities
{
    public class OrderItem : TransactionEntity
    {
        public Guid SubOrderId { get; set; }
        public virtual SubOrder SubOrder { get; set; } = null!;

        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        

    }
}
