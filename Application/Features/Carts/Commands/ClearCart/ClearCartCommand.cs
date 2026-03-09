using MediatR;
using Domain.Common;

namespace Application.Features.Carts.Commands.ClearCart
{
    public record ClearCartCommand : IRequest<Result<Unit>>;
}