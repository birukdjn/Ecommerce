using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Commands.ApproveProduct
{
    public record ApproveProductCommand(Guid ProductId) : IRequest<Result<Unit>>;

}