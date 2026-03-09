using Application.DTOs.Cart;
using Application.Features.Carts.Commands.AddToCart;
using Application.Features.Carts.Commands.ClearCart;
using Application.Features.Carts.Commands.RemoveFromCart;
using Application.Features.Carts.Commands.ToggleCartItemSelection;
using Application.Features.Carts.Commands.UpdateCartItem;
using Application.Features.Carts.Commands.ValidateCart;
using Application.Features.Carts.Queries.GetCart;
using Application.Features.Carts.Queries.GetCartCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/cart")]
    public class CartsController(ISender mediator) : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetCart()
            => HandleResult(await mediator.Send(new GetCartQuery()));

        [HttpPost("items")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartCommand command)
            => HandleResult(await mediator.Send(command));

        [HttpPut("items/{id}")]
        public async Task<ActionResult> UpdateQuantity(Guid id, [FromBody] UpdateCartItemDto dto)
            => HandleResult(await mediator.Send(new UpdateCartItemCommand(id, dto.Quantity)));

        [HttpDelete("items/{id}")]
        public async Task<ActionResult> RemoveItem(Guid id)
            => HandleResult(await mediator.Send(new RemoveFromCartCommand(id)));

        [HttpDelete("clear")]
        public async Task<ActionResult> ClearCart()
            => HandleResult(await mediator.Send(new ClearCartCommand()));

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount()
            => HandleResult(await mediator.Send(new GetCartCountQuery()));

        [HttpPost("validate")]
        public async Task<ActionResult<CartValidationDto>> Validate()
            => HandleResult(await mediator.Send(new ValidateCartCommand()));

        [HttpPatch("items/{id}/select")]
        public async Task<ActionResult> ToggleSelect(Guid id, [FromBody] bool isSelected)
            => HandleResult(await mediator.Send(new ToggleSelectionCommand(id, isSelected)));
    }
}