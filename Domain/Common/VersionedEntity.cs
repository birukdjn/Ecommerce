using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Common
{
    public abstract class VersionedEntity : AuditableEntity
    {
        [Timestamp]
        [Column("xmin")]
        public uint RowVersion { get; set; }
    }
}