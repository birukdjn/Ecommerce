using Api.Controllers;
using Application.Features.Admins.Transaction.Commands.ProcessPayout;
using Application.Features.Admins.Transaction.Queries.GetPendingPayouts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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