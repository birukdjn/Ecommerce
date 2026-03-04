using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Queries.GetProductImages
{
    public record GetProductImagesQuery(Guid ProductId) : IRequest<Result<List<ProductImageDto>>>;

}