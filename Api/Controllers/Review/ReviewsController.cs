using Api.Controllers.Common;
using Application.DTOs.Review;
using Application.Features.Reviews.Commands.CreateReview;
using Application.Features.Reviews.Commands.DeleteReview;
using Application.Features.Reviews.Commands.UpdateReview;
using Application.Features.Reviews.Queries.GetProductReview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Review
{
    [Authorize]
    [Route("api/reviews")]
    public class ReviewsController(ISender mediator) : ApiControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateReviewCommand command)
            => HandleResult(await mediator.Send(command));

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateReviewDto request)
            => HandleResult(await mediator.Send(new UpdateReviewCommand(id, request.Rating, request.Comment)));

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
            => HandleResult(await mediator.Send(new DeleteReviewCommand(id)));

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetByProduct(Guid productId)
            => HandleResult(await mediator.Send(new GetProductReviewsQuery(productId)));
    }
}