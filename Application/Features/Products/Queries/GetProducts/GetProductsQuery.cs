using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts
{
    public record GetProductsQuery(ProductSpecParams Params) : IRequest<Result<PagedList<ProductDto>>>;

}