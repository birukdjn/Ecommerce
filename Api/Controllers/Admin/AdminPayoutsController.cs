using Api.Controllers.Common;
using Application.Features.Admins.Commands.ProcessPayout;
using Application.Features.Admins.Queries.GetPendingPayouts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin
{

    [Authorize(Roles = "Admin")]
    [Route("api/admin/payouts")]
    public class AdminPayoutsController(ISender mediator) : ApiControllerBase
    {
        [HttpGet("pending")]
        public async Task<ActionResult> GetPendingPayouts()
            => HandleResult(await mediator.Send(new GetPendingPayoutsQuery()));

        [HttpPost("process")]
        public async Task<ActionResult> ProcessPayout([FromBody] ProcessPayoutCommand command)
            => HandleResult(await mediator.Send(command));
    }
}