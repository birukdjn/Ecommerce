using Application.DTOs;
using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Commands.DeleteCategory;
using Application.Features.Categories.Commands.UpdateCategory;
using Application.Features.Categories.Queries.GetCategories;
using Application.Features.Categories.Queries.GetCategoryById;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController(ISender mediator) : ControllerBase
    {
        [HttpGet("categories")]
        public async Task<ActionResult> GetCategories()
           => await Handle(new GetCategoriesQuery());

        [HttpGet("categories/{id}")]
        public async Task<ActionResult> GetCategory(Guid id)
            => await Handle(new GetCategoryByIdQuery(id));

        [HttpPost("categories")]
        public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
            => await Handle(command);

        [HttpPut("categories/{id}")]
        public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest dto)
            => await Handle(new UpdateCategoryCommand(id, dto.Name, dto.Description));

        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
            => await Handle(new DeleteCategoryCommand(id));


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