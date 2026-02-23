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
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController(ISender mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressCommand command)
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
            => Ok(await mediator.Send(new GetAddressesQuery()));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(await mediator.Send(new GetAddressByIdQuery(id)));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAddressCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");
            var result = await mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id) => Ok(await mediator.Send(new DeleteAddressCommand(id)));

        [HttpPatch("{id}/set-default")]
        public async Task<IActionResult> SetDefault(Guid id) => Ok(await mediator.Send(new SetDefaultAddressCommand(id)));
    }
}