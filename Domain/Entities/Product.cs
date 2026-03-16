
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Product : VersionedEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;
        public ProductStatus Status { get; set; } = ProductStatus.Pending;
        public string? RejectionReason { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; } = [];
        public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
        public virtual ICollection<Review> Reviews { get; set; } = [];
        public virtual ICollection<ProductCategory> ProductCategories { get; set; } = [];


        public decimal GetCommissionPercentage()
        {
            return ProductCategories?
                .FirstOrDefault()?
                .Category?
                .CommissionPercentage ?? 0;
        }

    }
}
