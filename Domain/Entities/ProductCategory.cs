
using Domain.Common;

namespace Domain.Entities
{
    public class ProductCategory:BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
    }
}
