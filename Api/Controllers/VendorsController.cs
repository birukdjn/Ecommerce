using Application.Features.Vendors.Commands.BecomeVendor;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/vendors")]
    public class VendorsController(ISender mediator) : ApiControllerBase
    {
        [HttpPost("apply")]
        public async Task<ActionResult> BecomeVendor([FromBody] BecomeVendorCommand command)
        {
            return HandleResult(await mediator.Send(command));
        }
    }
}