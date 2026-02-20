
using Domain.Common;

namespace Domain.Entities
{
    public class Product:BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;
        public bool IsApproved { get; set; } = false;
  
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
