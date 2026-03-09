using Domain.Common;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveFromCart
{
    public record RemoveFromCartCommand(Guid CartItemId) : IRequest<Result<Unit>>;

}