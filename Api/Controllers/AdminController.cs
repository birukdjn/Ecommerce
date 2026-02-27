using Application.DTOs;
using Application.Features.Admins.Commands.ApproveVendor;
using Application.Features.Admins.Commands.RejectVendor;
using Application.Features.Admins.Commands.ToggleVendorStatus;
using Application.Features.Admins.Queries.GetStats;
using Application.Features.Admins.Queries.GetUsers;
using Application.Features.Admins.Queries.GetVendorRequests;
using Application.Features.Admins.Queries.GetVendors;
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

        // --- VENDOR REQUESTS (ONBOARDING) ---
        [HttpGet("vendors/requests")]
        public async Task<ActionResult> GetVendorRequests()
            => await Handle(new GetVendorRequestsQuery());

        [HttpGet("vendors/requests/{id}")]
        public async Task<ActionResult> GetVendorRequest(Guid id)
            => await Handle(new GetVendorRequestByIdQuery(id));

        [HttpPost("vendors/{id}/approve")]
        public async Task<ActionResult> ApproveVendor(Guid id)
            => await Handle(new ApproveVendorCommand(id));

        [HttpPost("vendors/{id}/reject")]
        public async Task<ActionResult> RejectVendor(Guid id, [FromBody] RejectVendorRequest request)
            => await Handle(new RejectVendorCommand(id, request.Reason));

        // --- VENDOR MANAGEMENT (EXISTING VENDORS) ---
        [HttpGet("vendors")]
        public async Task<ActionResult> GetAllVendors()
            => await Handle(new GetAllVendorsQuery());

        [HttpPatch("vendors/{id}/status")]
        public async Task<ActionResult> ToggleVendorStatus(Guid id, [FromQuery] bool isActive)
            => await Handle(new ToggleVendorStatusCommand(id, isActive));

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