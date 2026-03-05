using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Commands.AddProductImage
{
    public record AddProductImageCommand
    (
        Guid ProductId,
        string ImageUrl,
        string? AltText) : IRequest<Result<Guid>>;
}