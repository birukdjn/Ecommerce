
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Review:BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public Guid CustomerId { get; set; }
        public virtual ApplicationUser Customer { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsApproved { get; set; } = false;
    }
}
