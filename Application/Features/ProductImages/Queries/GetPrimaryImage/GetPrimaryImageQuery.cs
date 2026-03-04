using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Queries.GetPrimaryImage
{
    public record GetPrimaryImageQuery(Guid ProductId) : IRequest<Result<ProductImageDto>>;
}