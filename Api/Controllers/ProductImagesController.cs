using Application.DTOs.Product;
using Application.Features.ProductImages.Commands.AddProductImage;
using Application.Features.ProductImages.Commands.BulkAddProductImage;
using Application.Features.ProductImages.Commands.DeleteProductImage;
using Application.Features.ProductImages.Commands.ReorderProductImage;
using Application.Features.ProductImages.Commands.RestoreProductImage;
using Application.Features.ProductImages.Commands.SetPrimaryImage;
using Application.Features.ProductImages.Commands.UpdateProductImage;
using Application.Features.ProductImages.Queries.GetPrimaryImage;
using Application.Features.ProductImages.Queries.GetProductImages;
using Application.Features.ProductImages.Queries.GetTrashedImages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/vendor/products/{productId:guid}/images")]
    [Authorize(policy: "VendorOnly")]
    public class ProductImagesController(ISender mediator) : ApiControllerBase
    {
        // Get all images for a specific product
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductImageDto>>> GetImages(Guid productId)
            => HandleResult(await mediator.Send(new GetProductImagesQuery(productId)));

        [HttpGet("primary")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductImageDto>> GetPrimaryImage(Guid productId)
            => HandleResult(await mediator.Send(new GetPrimaryImageQuery(productId)));

        [HttpGet("trashed")]
        public async Task<ActionResult<List<ProductImageDto>>> GetTrashedImages(Guid productId)
            => HandleResult(await mediator.Send(new GetTrashedImagesQuery(productId)));

        // Add new image URL
        [HttpPost]
        public async Task<ActionResult<Guid>> AddImage(Guid productId, [FromBody] AddProductImageDto dto)
            => HandleResult(await mediator.Send(new AddProductImageCommand(productId, dto.Url, dto.AltText)));

        // Add bulk images
        [HttpPost("bulk")]
        public async Task<ActionResult> BulkAddImages(Guid productId, [FromBody] List<AddProductImageDto> dtos)
            => HandleResult(await mediator.Send(new BulkAddProductImagesCommand(productId, dtos)));

        // Set Primary Image
        [HttpPatch("{id:guid}/set-primary")]
        public async Task<ActionResult> SetPrimary(Guid productId, Guid id)
            => HandleResult(await mediator.Send(new SetPrimaryImageCommand(productId, id)));


        // reorder images
        [HttpPatch("reorder")]
        public async Task<ActionResult> ReorderImages(Guid productId, [FromBody] List<Guid> orderedIds)
                => HandleResult(await mediator.Send(new ReorderProductImagesCommand(productId, orderedIds)));

        // Update image URL
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateImage(Guid productId, Guid id, [FromBody] UpdateProductImageDto dto)
            => HandleResult(await mediator.Send(new UpdateProductImageCommand(productId, id, dto.Url, dto.AltText)));

        // Delete image 
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteImage(Guid productId, Guid id)
            => HandleResult(await mediator.Send(new DeleteProductImageCommand(productId, id)));

        [HttpPatch("{id:guid}/restore")]
        public async Task<ActionResult> RestoreImage(Guid productId, Guid id)
            => HandleResult(await mediator.Send(new RestoreProductImageCommand(productId, id)));

    }
}
