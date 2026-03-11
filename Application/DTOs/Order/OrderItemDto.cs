namespace Application.DTOs.Order
{
    public record OrderItemDto(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        string? ProductImageUrl
    );
}