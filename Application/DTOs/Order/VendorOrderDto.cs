namespace Application.DTOs.Order
{
    public record VendorOrderDto(
        Guid SubOrderId,
        string MasterOrderNumber,
        decimal SubTotal,
        decimal PlatformCommission,
        decimal YourEarnings,
        string Status,
        DateTime CreatedAt,
        string CustomerName,
        List<OrderItemDto> Items
    );
}