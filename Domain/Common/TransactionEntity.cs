using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Common
{
    public abstract class TransactionEntity : BaseEntity
    {
        [Timestamp]
        [Column("xmin")]
        public uint RowVersion { get; set; }

    }
}
