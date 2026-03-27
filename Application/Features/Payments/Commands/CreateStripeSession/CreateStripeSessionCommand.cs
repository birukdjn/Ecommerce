using Domain.Common;
using MediatR;

namespace Application.Features.Payments.Commands.CreateStripeSession
{
    public record CreateStripeSessionCommand(Guid OrderId) : IRequest<Result<string>>;
}