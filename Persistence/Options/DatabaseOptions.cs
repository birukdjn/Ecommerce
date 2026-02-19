
using System.ComponentModel.DataAnnotations;

namespace Persistence.Options
{
    public class DatabaseOptions
    {
        [Required]
        public required string ConnectionString { get; set; }
    }
}
