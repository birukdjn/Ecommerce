using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Commands.DeleteProductImage
{
    public record DeleteProductImageCommand
    (
        Guid ProductId,
        Guid ImageId) : IRequest<Result<Unit>>;

}