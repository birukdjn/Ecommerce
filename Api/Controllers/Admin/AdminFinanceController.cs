using Api.Controllers.Common;
using Application.Features.Admins.Queries.GetPlatformFinanceSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin
{

    [Authorize(Roles = "Admin")]
    [Route("api/admin/finance")]
    public class AdminFinanceController(ISender mediator) : ApiControllerBase
    {
        // 1. Get the "Big Picture" (Total Sales, Total Commission, Total in Escrow)
        [HttpGet("summary")]
        public async Task<ActionResult> GetFinanceSummary()
            => HandleResult(await mediator.Send(new GetPlatformFinanceSummaryQuery()));

    }
}