using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.Products.Queries.GetTrendingProducts
{
    public record GetTrendingProductsQuery : IRequest<Result<List<ProductDto>>>;

}