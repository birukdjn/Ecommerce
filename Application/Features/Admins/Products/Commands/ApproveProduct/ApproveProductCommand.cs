using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Products.Commands.ApproveProduct
{
    public record ApproveProductCommand(Guid ProductId) : IRequest<Result<Unit>>;

}