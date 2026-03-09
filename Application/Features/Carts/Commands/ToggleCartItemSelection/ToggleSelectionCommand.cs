using Domain.Common;
using MediatR;

namespace Application.Features.Carts.Commands.ToggleCartItemSelection
{
    public record ToggleSelectionCommand(Guid CartItemId, bool IsSelected) : IRequest<Result<Unit>>;
}