using Api.Controllers.Common;
using Application.DTOs.Product;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Commands.UpdateProductStock;
using Application.Features.Products.Queries.GetMyProductDetail;
using Application.Features.Products.Queries.GetMyProducts;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Product;

[Route("api/vendor/products")]
[Authorize(Roles = "Vendor")]
public class VendorProductsController(ISender mediator) : ApiControllerBase
{
    /// product list (paginated)
    [HttpGet]
    public async Task<ActionResult<PagedList<MyProductSummaryDto>>> GetMyProducts(
        [FromQuery] ProductSpecParams specParams)
        => HandleResult(await mediator.Send(new GetMyProductsQuery(specParams)));

    /// product detail
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MyProductDetailDto>> GetMyProduct(Guid id)
        => HandleResult(await mediator.Send(new GetMyProductDetailQuery(id)));

    /// Create product
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateProduct(
        [FromBody] CreateProductCommand command)
        => HandleResult(await mediator.Send(command));

    /// Update product
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        return HandleResult(await mediator.Send(command));
    }

    /// Update stock
    [HttpPatch("{id:guid}/stock")]
    public async Task<ActionResult> UpdateStock(
        Guid id,
        [FromBody] UpdateStockDto dto)
        => HandleResult(await mediator.Send(
            new UpdateProductStockCommand(id, dto.Quantity)));

    /// Delete product
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteProduct(Guid id)
        => HandleResult(await mediator.Send(new DeleteProductCommand(id)));
}