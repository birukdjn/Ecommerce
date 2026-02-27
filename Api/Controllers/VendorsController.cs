using Application.Features.Vendors.Commands.BecomeVendor;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{

    [Authorize()]
    [ApiController]
    [Route("api/[controller]")]
    public class VendorsController(IMediator mediator) : ControllerBase
    {
        
        [HttpPost("apply")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BecomeVendor([FromBody] BecomeVendorCommand command)
        { 
            var result = await mediator.Send(command);

            if (!result.IsSuccess) return BadRequest(result);
        
            return Ok(result);
            
        }
    }
}