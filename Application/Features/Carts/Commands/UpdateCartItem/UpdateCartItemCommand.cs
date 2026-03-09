using MediatR;
using Domain.Common;

namespace Application.Features.Carts.Commands.UpdateCartItem
{
    public record UpdateCartItemCommand(Guid CartItemId, int Quantity) : IRequest<Result<Unit>>;
}