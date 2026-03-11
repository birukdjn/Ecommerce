namespace Application.DTOs.Order;

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    int ItemCount
);