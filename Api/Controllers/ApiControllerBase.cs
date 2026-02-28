using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
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
                // If it's a boolean 'true' (common for Deletes/Updates), return 204 No Content
                if (result.Value is bool b && b) return NoContent();
                
                // If there's no value, return 204 No Content
                if (result.Value == null) return NoContent();

                return Ok(result.Value);
            }

            // Error Handling logic
            if (result.Error != null && result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }
    }
}