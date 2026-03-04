namespace Application.DTOs.Product
{
    public record UpdateProductImageDto(string Url, string? AltText = null);
}