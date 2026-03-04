using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Commands.UpdateProductImage
{
    public record UpdateProductImageCommand(Guid ProductId, Guid ImageId, string NewUrl, string? AltText) : IRequest<Result<Unit>>;

}