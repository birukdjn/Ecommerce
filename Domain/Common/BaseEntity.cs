using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
