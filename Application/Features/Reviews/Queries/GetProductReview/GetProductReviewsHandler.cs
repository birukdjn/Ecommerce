using Application.DTOs.Review;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reviews.Queries.GetProductReview
{
    public class GetProductReviewsHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetProductReviewsQuery, Result<IReadOnlyList<ReviewDto>>>
    {
        public async Task<Result<IReadOnlyList<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await unitOfWork.Repository<Review>().Query()
                .Where(r => r.ProductId == request.ProductId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto(
                    r.Id,
                    r.Customer.FullName ?? "Anonymous",
                    r.Customer.ProfilePictureUrl,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt))
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<ReviewDto>>.Success(reviews);
        }
    }
}