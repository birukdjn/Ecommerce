using Domain.Common;

namespace Domain.Entities
{
    public class Cart:BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;


        public virtual ICollection<CartItem> Items { get; set; } = [];
    }
}
