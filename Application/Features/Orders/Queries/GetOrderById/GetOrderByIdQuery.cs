using Application.DTOs.Order;
using Domain.Common;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById
{
    public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDetailsDto>>;
}