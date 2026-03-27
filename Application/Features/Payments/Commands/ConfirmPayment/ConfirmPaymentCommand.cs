using Domain.Common;
using MediatR;

namespace Application.Features.Payments.Commands.ConfirmPayment
{
    public record ConfirmPaymentCommand(Guid OrderId, string ProviderTransactionId) : IRequest<Result>;
}