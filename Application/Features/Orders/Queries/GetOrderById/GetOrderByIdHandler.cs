using Application.DTOs.Address;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsDto>>
{
    public async Task<Result<OrderDetailsDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId == null || userId == Guid.Empty)
            return Result<OrderDetailsDto>.Failure("Unauthorized");

        var order = await unitOfWork.Repository<Order>().Query()
            .Include(o => o.SubOrders)
                .ThenInclude(s => s.Vendor)
            .Include(o => o.SubOrders)
                .ThenInclude(s => s.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.CustomerId == userId, cancellationToken);

        if (order == null)
            return Result<OrderDetailsDto>.Failure("Order not found.");

        // Mapping Entity to DTO
        var dto = new OrderDetailsDto(
            order.Id,
            order.OrderNumber,
            order.TotalAmount,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.CreatedAt,
            new AddressSnapshotDto(
                order.ShippingFullName,
                order.ShippingPhoneNumber,
                order.ShippingCountry,
                order.ShippingRegion,
                order.ShippingCity,
                order.ShippingSpecialPlaceName
            ),
            order.SubOrders.Select(s => new SubOrderDto(
                s.Id,
                s.Vendor.StoreName,
                s.SubTotal,
                s.Status.ToString(),
                s.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    i.Product.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                )).ToList()
            )).ToList()
        );

        return Result<OrderDetailsDto>.Success(dto);
    }
}