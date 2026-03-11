using Application.DTOs.Order;
using Domain.Common;
using MediatR;

namespace Application.Features.Orders.Queries.GetVendorOrders
{
    public record GetVendorOrdersQuery : IRequest<Result<List<VendorOrderDto>>>;
}