using Api.Controllers.Common;
using Application.DTOs.Product;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using Application.Features.Products.Queries.GetProductsByCategory;
using Application.Features.Products.Queries.GetTrendingProducts;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Product;

[Route("api/products")]
[AllowAnonymous]
public class ProductsController(ISender mediator) : ApiControllerBase
{
    /// Public product catalog with filtering/paging
    [HttpGet]
    public async Task<ActionResult<PagedList<ProductDto>>> GetProducts(
        [FromQuery] ProductSpecParams specParams)
        => HandleResult(await mediator.Send(new GetProductsQuery(specParams)));

    /// Public product detail
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        => HandleResult(await mediator.Send(new GetProductByIdQuery(id)));

    /// Trending products
    [HttpGet("trending")]
    public async Task<ActionResult<List<ProductDto>>> GetTrending()
        => HandleResult(await mediator.Send(new GetTrendingProductsQuery()));

    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<PagedList<ProductDto>>> GetByCategory(
        Guid categoryId,
        [FromQuery] ProductSpecParams specParams)
        => HandleResult(await mediator.Send(new GetProductsByCategoryQuery(categoryId, specParams)));
}