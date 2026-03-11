using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Transaction.Commands.ProcessPayout
{

    public record ProcessPayoutCommand(
        Guid PayoutRequestId,
        bool Approved,
        string? BankReference = null,
        string? Note = null) : IRequest<Result<Unit>>;
}