using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Commands.RestoreProductImage
{
    public record RestoreProductImageCommand(Guid ProductId, Guid ImageId) : IRequest<Result<Unit>>;
}