using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.GetMyOrders
{
    public class GetMyOrdersHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<GetMyOrdersQuery, Result<List<OrderSummaryDto>>>
    {
        public async Task<Result<List<OrderSummaryDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<List<OrderSummaryDto>>.Failure("Unauthorized");

            var orders = await unitOfWork.Repository<Order>().Query()
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OrderSummaryDto(
                    o.Id,
                    o.OrderNumber,
                    o.TotalAmount,
                    o.Status.ToString(),
                    o.CreatedAt,
                    o.OrderItems.Count + o.SubOrders.SelectMany(s => s.Items).Count()
                ))
                .ToListAsync(cancellationToken);

            return Result<List<OrderSummaryDto>>.Success(orders);
        }
    }
}