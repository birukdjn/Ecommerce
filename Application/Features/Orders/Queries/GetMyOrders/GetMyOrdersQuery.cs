using Application.DTOs.Order;
using Domain.Common;
using MediatR;

namespace Application.Features.Orders.Queries.GetMyOrders
{
    public record GetMyOrdersQuery(int PageNumber, int PageSize) : IRequest<Result<List<OrderSummaryDto>>>;
}