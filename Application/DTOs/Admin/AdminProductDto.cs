using Domain.Enums;

namespace Application.DTOs.Admin
{
    public class AdminProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string VendorStoreName { get; set; } = null!;
        public decimal Price { get; set; }
        public ProductStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Categories { get; set; } = [];
        public string? RejectionReason { get; set; }
    }
}