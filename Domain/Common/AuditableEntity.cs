namespace Domain.Common
{
    public abstract class AuditableEntity : BaseEntity
    {
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; } = DateTime.UtcNow;
    }
}
