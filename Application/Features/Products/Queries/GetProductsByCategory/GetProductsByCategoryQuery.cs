using Domain.Common;
using MediatR;
using Application.DTOs.Product;

namespace Application.Features.Products.Queries.GetProductsByCategory;

public record GetProductsByCategoryQuery(Guid CategoryId, ProductSpecParams Params)
    : IRequest<Result<PagedList<ProductDto>>>;