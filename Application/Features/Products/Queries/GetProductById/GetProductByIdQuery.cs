using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById
{
    public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDetailDto>>;
}