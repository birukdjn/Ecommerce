using Application.Features.Admins.Commands.ApproveVendor;
using Application.Features.Admins.Commands.RejectVendor;
using Application.Features.Admins.Queries.GetVendorRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

namespace Api.Controllers
{

    [Authorize(Policy = "AdminOnly")]
    [Route("api/admin/vendors")]
    public class AdminController(ISender mediator) :ControllerBase
    {
        [HttpGet("requests")]
        public async Task<ActionResult> GetRequests()
        {
            var result = await mediator.Send(new GetVendorRequestsQuery());
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        

        [HttpGet("requests/{id}")]
        public async Task<ActionResult> GetRequest(Guid id)
        {
            var result = await mediator.Send(new GetVendorRequestByIdQuery(id));
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        

        [HttpPost("{id}/approve")]
        public async Task<ActionResult<bool>> Approve(Guid id)
        {
            var result = await mediator.Send(new ApproveVendorCommand(id));

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult> Reject(Guid id, [FromBody] RejectVendorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest("A reason for rejection must be provided.");

            var result = await mediator.Send(new RejectVendorCommand(id, request.Reason));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}
