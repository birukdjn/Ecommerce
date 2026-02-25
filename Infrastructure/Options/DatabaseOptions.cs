
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options
{
    public class DatabaseOptions
    {
        public const string SectionName = "DatabaseOptions";
        [Required]
        public required string ConnectionString { get; set; }
    }
}
