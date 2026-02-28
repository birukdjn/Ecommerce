using Application.Features.Profile.Commands.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/profile")]
    public class ProfileController(ISender mediator) : ApiControllerBase
    {
        [HttpPut]
        public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
            => HandleResult(await mediator.Send(command));
    }
}