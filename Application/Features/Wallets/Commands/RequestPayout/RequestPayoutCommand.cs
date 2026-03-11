using Domain.Common;
using MediatR;

namespace Application.Features.Wallets.Commands.RequestPayout
{
    public record RequestPayoutCommand(decimal Amount) : IRequest<Result<Guid>>;
}