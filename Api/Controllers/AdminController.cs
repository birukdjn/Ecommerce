using Application.Features.Admins.Queries.GetStats;
using Application.Features.Admins.Queries.GetUsers;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController(ISender mediator) : ControllerBase
    {
        // --- SYSTEM STATS ---
        [HttpGet("stats")]
        public async Task<ActionResult> GetDashboardStats()
            => await Handle(new GetAdminDashboardStatsQuery());


        // --- USER MANAGEMENT ---
        [HttpGet("users")]
        public async Task<ActionResult> GetAllUsers()
            => await Handle(new GetAllUsersQuery());

        // --- PRIVATE HELPER ---
        private async Task<ActionResult> Handle<T>(IRequest<Result<T>> request)
        {
            var result = await mediator.Send(request);

            if (!result.IsSuccess)
            {
                // Handle 404 vs 400
                if (result.Error != null && result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result.Error);
                }
                return BadRequest(result.Error);
            }

            // Return 204 No Content for successful void/bool results, 200 for data
            if (result.Value is bool b && b) return NoContent();

            return Ok(result.Value);
        }
    }
}