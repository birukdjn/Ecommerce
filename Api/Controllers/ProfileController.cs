using Application.Features.Profile.Commands.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController(ISender mediator) : ControllerBase
    {
        [HttpPost("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
        
    }
}