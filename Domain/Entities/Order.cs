using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Order:BaseEntity
    {
        public Guid CustomerId { get; set; }
        public virtual ApplicationUser Customer { get; set; } = null!;

        public required string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        // Shipping snapshot
        public string ShippingFullName { get; set; } = null!;
        public string ShippingPhoneNumber { get; set; } = null!;
        public string ShippingCountry { get; set; } = null!;
        public string ShippingRegion { get; set; } = null!;
        public string ShippingCity { get; set; } = null!;
        public string ShippingSpecialPlaceName { get; set; } = null!;


        public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
        public virtual ICollection<SubOrder> SubOrders { get; set; } = [];

    }
}
