using Domain.Common;
using Domain.Enums;
using MediatR;

namespace Application.Features.Orders.Commands.UpdateSubOrderStatus
{
    public record UpdateSubOrderStatusCommand(Guid SubOrderId, SubOrderStatus NewStatus) : IRequest<Result<Unit>>;
}