using Application.DTOs.Address;

namespace Application.DTOs.Order
{
    public record OrderDetailsDto(
        Guid Id,
        string OrderNumber,
        decimal TotalAmount,
        string Status,
        string PaymentStatus,
        DateTime CreatedAt,
        AddressSnapshotDto ShippingAddress,
        List<SubOrderDto> SubOrders
    );
}