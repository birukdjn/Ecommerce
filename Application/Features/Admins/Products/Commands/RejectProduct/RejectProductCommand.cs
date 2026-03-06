using Domain.Common;
using MediatR;
namespace Application.Features.Admins.Products.Commands.RejectProduct
{
    public record RejectProductCommand(Guid ProductId, string Reason) : IRequest<Result<Unit>>;
}
