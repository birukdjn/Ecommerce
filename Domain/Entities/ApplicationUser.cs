using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Vendor? Vendor { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = [];
        public virtual ICollection<Review> Reviews { get; set; } = [];
        public virtual ICollection<Cart> Carts { get; set; } = [];

    }
}
