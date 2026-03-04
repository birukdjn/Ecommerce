using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Queries.GetTrashedImages
{
    public record GetTrashedImagesQuery(Guid ProductId) : IRequest<Result<List<ProductImageDto>>>;
}