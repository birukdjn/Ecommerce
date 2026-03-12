using Domain.Common;
using MediatR;
namespace Application.Features.Admins.Commands.RejectProduct
{
    public record RejectProductCommand(Guid ProductId, string Reason) : IRequest<Result<Unit>>;
}
