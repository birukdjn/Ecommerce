using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Application.Features.Orders.Queries.GetMyOrders
{
    public class GetMyOrdersHandler(IUnitOfWork unitOfWork,
        IDistributedCache distributedCache,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetMyOrdersQuery, Result<List<OrderSummaryDto>>>
    {
        public async Task<Result<List<OrderSummaryDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<List<OrderSummaryDto>>.Failure("Unauthorized");

            string cacheKey = $"orders_{userId}_p{request.PageNumber}";

            var cachedData = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return Result<List<OrderSummaryDto>>.Success(JsonSerializer.Deserialize<List<OrderSummaryDto>>(cachedData)!);
            }

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

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(orders), cacheOptions, cancellationToken);

            return Result<List<OrderSummaryDto>>.Success(orders);
        }
    }
}