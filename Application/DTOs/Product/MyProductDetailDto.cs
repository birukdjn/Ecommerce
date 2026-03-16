using Domain.Enums;

namespace Application.DTOs.Product
{

    public class MyProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Detailed collections
        public List<string> ImageUrls { get; set; } = [];
        public List<Guid> CategoryIds { get; set; } = [];
        public List<string> CategoryNames { get; set; } = [];
    }
}