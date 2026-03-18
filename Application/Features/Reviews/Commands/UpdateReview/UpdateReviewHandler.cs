using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;

namespace Application.Features.Reviews.Commands.UpdateReview
{
    public class UpdateReviewHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<UpdateReviewCommand, Result>
    {
        public async Task<Result> Handle(UpdateReviewCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();

            var review = await unitOfWork.Repository<Review>().GetByIdAsync(command.ReviewId);

            if (review == null) return Result.Failure("Review not found.");

            if (review.CustomerId != userId) return Result.Failure("Unauthorized.");

            if (command.Rating.HasValue)
            {
                if (command.Rating < 1 || command.Rating > 5) return Result.Failure("Rating must be between 1 and 5.");
                review.Rating = command.Rating.Value;
            }

            if (command.Comment != null)
            {
                review.Comment = command.Comment;
            }

            review.IsApproved = true;

            unitOfWork.Repository<Review>().Update(review);
            await unitOfWork.Complete();

            return Result.Success();
        }
    }
}