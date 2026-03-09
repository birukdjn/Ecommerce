using MediatR;
using Domain.Common;

namespace Application.Features.Carts.Commands.AddToCart
{
    public record AddToCartCommand(Guid ProductId, int Quantity) : IRequest<Result<Guid>>;
}