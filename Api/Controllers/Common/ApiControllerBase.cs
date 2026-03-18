using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null) return NotFound();

            if (result.IsSuccess)
            {
                if (result.Value == null) return NoContent();
                return Ok(result.Value);
            }

            return ProcessError(result.Error);
        }

        protected ActionResult HandleResult(Result result)
        {
            if (result == null) return NotFound();

            if (result.IsSuccess) return NoContent();

            return ProcessError(result.Error);
        }

        private ActionResult ProcessError(string? error)
        {
            if (error != null && error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(error);
            }

            return BadRequest(error);
        }
    }
}