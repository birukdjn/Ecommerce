namespace Application.DTOs.Order
{
    public record SubOrderDto
    (
        Guid Id,
        string VendorStoreName,
        decimal SubTotal,
        string Status,
        List<OrderItemDto> Items
    );
}