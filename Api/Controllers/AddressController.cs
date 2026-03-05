using Application.DTOs.Address;
using Application.Features.Addresses.Commands.CreateAddress;
using Application.Features.Addresses.Commands.DeleteAddress;
using Application.Features.Addresses.Commands.RestoreAddress;
using Application.Features.Addresses.Commands.SetDefaultAddress;
using Application.Features.Addresses.Commands.UpdateAddress;
using Application.Features.Addresses.Queries.GetAddressById;
using Application.Features.Addresses.Queries.GetAddresses;
using Application.Features.Addresses.Queries.GetDefaultAddress;
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

        // Get all addresses for current user
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetMyAddresses()
            => HandleResult(await mediator.Send(new GetAddressesQuery()));

        // Get a specific address by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<AddressDto>> GetById(Guid id)
            => HandleResult(await mediator.Send(new GetAddressByIdQuery(id)));

        // Get the default address
        [HttpGet("default")]
        public async Task<ActionResult<AddressDto>> GetDefault()
            => HandleResult(await mediator.Send(new GetDefaultAddressQuery()));

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateAddressCommand command)
        {
            command = command with { Id = id };
            return HandleResult(await mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
            => HandleResult(await mediator.Send(new DeleteAddressCommand(id)));

        [HttpPatch("{id}/restore")]
        public async Task<ActionResult> Restore(Guid id)
        => HandleResult(await mediator.Send(new RestoreAddressCommand(id)));

        [HttpPatch("{id}/default")]
        public async Task<ActionResult> SetDefault(Guid id)
            => HandleResult(await mediator.Send(new SetDefaultAddressCommand(id)));
    }
}