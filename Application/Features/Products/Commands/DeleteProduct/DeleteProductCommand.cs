using Domain.Common;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : IRequest<Result<bool>>;

}