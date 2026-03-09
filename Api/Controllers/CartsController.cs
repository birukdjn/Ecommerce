using Application.DTOs.Cart;
using Application.Features.Carts.Commands.AddToCart;
using Application.Features.Carts.Commands.ClearCart;
using Application.Features.Carts.Commands.RemoveFromCart;
using Application.Features.Carts.Commands.UpdateCartItem;
using Application.Features.Carts.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize] // Requires a valid JWT token
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
    }
}