using Application.Features.Addresses.Commands.CreateAddress;
using Application.Features.Addresses.Commands.DeleteAddress;
using Application.Features.Addresses.Commands.SetDefaultAddress;
using Application.Features.Addresses.Commands.UpdateAddress;
using Application.Features.Addresses.Queries.GetAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers
{
    [Authorize]
    [Route("api/addresses")]
    public class AddressController(ISender mediator) : ApiControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateAddressCommand command)
            => HandleResult(await mediator.Send(command));

        [HttpGet]
        public async Task<ActionResult> GetMyAddresses()
            => HandleResult(await mediator.Send(new GetAddressesQuery()));

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
            => HandleResult(await mediator.Send(new GetAddressByIdQuery(id)));

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateAddressCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");

            return HandleResult(await mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
            => HandleResult(await mediator.Send(new DeleteAddressCommand(id)));

        [HttpPatch("{id}/default")]
        public async Task<ActionResult> SetDefault(Guid id)
            => HandleResult(await mediator.Send(new SetDefaultAddressCommand(id)));
    }
}