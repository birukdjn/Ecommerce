using Api.Controllers;
using Application.Features.Orders.Commands.Checkout;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Orders.Queries.GetOrderById;
using Application.Features.Orders.Queries.GetMyOrders;
using Application.Features.Orders.Queries.GetVendorOrders;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/orders")]
    public class OrdersController(ISender mediator) : ApiControllerBase
    {
        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout([FromBody] CheckoutCommand command)
            => HandleResult(await mediator.Send(command));

        [HttpGet]
        public async Task<ActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => HandleResult(await mediator.Send(new GetMyOrdersQuery(pageNumber, pageSize)));

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrder(Guid id)
            => HandleResult(await mediator.Send(new GetOrderByIdQuery(id)));

        [HttpGet("vendor")]
        [Authorize(Policy = "VendorOnly")]
        public async Task<ActionResult> GetVendorOrders()
            => HandleResult(await mediator.Send(new GetVendorOrdersQuery()));
    }
}