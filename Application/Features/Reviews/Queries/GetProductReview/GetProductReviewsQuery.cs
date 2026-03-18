using Application.DTOs.Review;
using Domain.Common;
using MediatR;

namespace Application.Features.Reviews.Queries.GetProductReview
{
    public record GetProductReviewsQuery(Guid ProductId) : IRequest<Result<IReadOnlyList<ReviewDto>>>;
}