using Domain.Common;

namespace Domain.Entities
{
    public class Address:BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public required string Country { get; set; }
        public required string Region { get; set; }
        public required string City { get; set; }
        public required string SpecialPlaceName { get; set; }
        public bool IsDefault { get; set; } = false;

    }
}
