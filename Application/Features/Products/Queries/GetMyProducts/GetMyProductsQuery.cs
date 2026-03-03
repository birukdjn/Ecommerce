using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.Products.Queries.GetMyProducts
{
    public record GetMyProductsQuery(ProductSpecParams Params) : IRequest<Result<PagedList<MyProductSummaryDto>>>;
}