using Application.DTOs.Product.Admin;
using Application.Features.Admins.Products.Commands.ApproveProduct;
using Application.Features.Admins.Products.Commands.RejectProduct;
using Application.Features.Admins.Products.Queries.GetAdminProducts;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers
{

    // [Authorize(Roles = "Admin")]
    [Route("api/admin/products")]
    public class AdminProductsController(ISender mediator) : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PagedList<AdminProductDto>>> GetAllProducts(
            [FromQuery] AdminProductSpecParams specParams)
            => HandleResult(await mediator.Send(new GetAdminProductsQuery(specParams)));

        [HttpGet("pending")]
        public async Task<ActionResult<PagedList<AdminProductDto>>> GetPending(
            [FromQuery] AdminProductSpecParams specParams)
        {
            specParams.IsApproved = false;
            specParams.IsDeleted = false;

            return HandleResult(await mediator.Send(new GetAdminProductsQuery(specParams)));
        }

        [HttpPatch("{id:guid}/approve")]
        public async Task<ActionResult> ApproveProduct(Guid id)
            => HandleResult(await mediator.Send(new ApproveProductCommand(id)));

        [HttpPatch("{id:guid}/reject")]
        public async Task<ActionResult> RejectProduct(Guid id, [FromBody] RejectProductDto dto)
            => HandleResult(await mediator.Send(new RejectProductCommand(id, dto.Reason)));

    }
}