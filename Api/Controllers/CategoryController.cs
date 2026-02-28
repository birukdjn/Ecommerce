using Application.DTOs;
using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Commands.DeleteCategory;
using Application.Features.Categories.Commands.UpdateCategory;
using Application.Features.Categories.Queries.GetCategories;
using Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/categories")]
    public class CategoryController(ISender mediator) : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetCategories()
           => HandleResult(await mediator.Send(new GetCategoriesQuery()));

        [HttpGet("{id}")]
        public async Task<ActionResult> GetCategory(Guid id)
            => HandleResult(await mediator.Send(new GetCategoryByIdQuery(id)));

        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
            => HandleResult(await mediator.Send(command));

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest dto)
            => HandleResult(await mediator.Send(new UpdateCategoryCommand(id, dto.Name, dto.Description)));

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
            => HandleResult(await mediator.Send(new DeleteCategoryCommand(id)));
    }
}