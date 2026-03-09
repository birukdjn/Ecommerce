namespace Domain.Common
{
    public abstract class VersionedEntity : AuditableEntity
    {
        public uint RowVersion { get; set; }
    }
}