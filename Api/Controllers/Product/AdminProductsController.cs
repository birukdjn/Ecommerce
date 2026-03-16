using Api.Controllers.Common;
using Application.DTOs.Admin;
using Application.Features.Admins.Commands.ApproveProduct;
using Application.Features.Admins.Commands.RejectProduct;
using Application.Features.Admins.Queries.GetAdminProducts;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Product
{

    [Authorize(Roles = "Admin")]
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
            specParams.ProductStatus = ProductStatus.Pending;
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