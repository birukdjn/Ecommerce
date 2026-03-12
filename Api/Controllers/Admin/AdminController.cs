using Application.Features.Admins.Commands.ToggleVendorStatus;
using Application.Features.Admins.Queries.GetStats;
using Application.Features.Admins.Queries.GetUsers;
using Application.Features.Admins.Queries.GetVendors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/admin")]
    public class AdminController(ISender mediator) : ApiControllerBase
    {
        // --- SYSTEM STATS ---
        [HttpGet("stats")]
        public async Task<ActionResult> GetDashboardStats()
        {
            var result = await mediator.Send(new GetAdminDashboardStatsQuery());
            return HandleResult(result);
        }

        // --- USER MANAGEMENT ---
        [HttpGet("users")]
        public async Task<ActionResult> GetAllUsers()
        {
            var result = await mediator.Send(new GetAllUsersQuery());
            return HandleResult(result);
        }
        [HttpGet("vendors")]
        public async Task<ActionResult> GetAllVendors()
           => HandleResult(await mediator.Send(new GetAllVendorsQuery()));

        [HttpPatch("vendors/{id}/status")]
        public async Task<ActionResult> ToggleVendorStatus(Guid id, [FromQuery] bool isActive)
            => HandleResult(await mediator.Send(new ToggleVendorStatusCommand(id, isActive)));

    }
}