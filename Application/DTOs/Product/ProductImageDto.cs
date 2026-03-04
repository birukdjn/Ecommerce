namespace Application.DTOs.Product;

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string? ImageUrl { get; set; }
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
}