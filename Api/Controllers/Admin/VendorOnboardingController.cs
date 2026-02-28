using Application.DTOs;
using Application.Features.Admins.Commands.ApproveVendor;
using Application.Features.Admins.Commands.RejectVendor;
using Application.Features.Admins.Queries.GetVendorRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/vendor-onboarding")]
    public class VendorOnboardingController(ISender mediator) : ApiControllerBase
    {
        // --- VENDOR REQUESTS (ONBOARDING) ---
        [HttpGet("requests")]
        public async Task<ActionResult> GetVendorRequests()
            => HandleResult(await mediator.Send(new GetVendorRequestsQuery()));

        [HttpGet("requests/{id}")]
        public async Task<ActionResult> GetVendorRequest(Guid id)
            => HandleResult(await mediator.Send(new GetVendorRequestByIdQuery(id)));

        [HttpPatch("requests/{id}/approve")]
        public async Task<ActionResult> ApproveVendor(Guid id)
            => HandleResult(await mediator.Send(new ApproveVendorCommand(id)));

        [HttpPatch("requests/{id}/reject")]
        public async Task<ActionResult> RejectVendor(Guid id, [FromBody] RejectVendorRequest request)
            => HandleResult(await mediator.Send(new RejectVendorCommand(id, request.Reason)));

    }
}