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
                if (result.Value is bool b && b) return NoContent();
                
                if (result.Value == null) return NoContent();

                return Ok(result.Value);
            }

            if (result.Error != null && result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }
    }
}