using Application.DTOs.Cart;
using Domain.Common;
using MediatR;


namespace Application.Features.Carts.Queries.GetCart
{
    public record GetCartQuery : IRequest<Result<CartDto>>;
}