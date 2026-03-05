using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Commands.BulkAddProductImage
{
    public record BulkAddProductImagesCommand
    (
        Guid ProductId,
        List<AddProductImageDto> Images) : IRequest<Result<Unit>>;
}