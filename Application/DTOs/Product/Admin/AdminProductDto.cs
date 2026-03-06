namespace Application.DTOs.Product.Admin
{
    public class AdminProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string VendorStoreName { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Categories { get; set; } = [];
        public string? RejectionReason { get; set; }
    }
}