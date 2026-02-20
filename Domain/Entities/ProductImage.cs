
using Domain.Common;

namespace Domain.Entities
{
    public class ProductImage:BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public bool IsPrimary { get; set; } = false;
    }
}
