using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options
{
    public class AfroSmsOptions
    {
        public const string SectionName = "AfroSmsOptions";

        [Required]
        public required string Token { get; set; }

        [Required]
        public required string IdentifierId { get; set; }

        [Required]
        public required string SenderName { get; set; }
    }
}
