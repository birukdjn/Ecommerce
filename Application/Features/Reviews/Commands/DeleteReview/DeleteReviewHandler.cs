using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;

namespace Application.Features.Reviews.Commands.DeleteReview
{
    public class DeleteReviewHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<DeleteReviewCommand, Result>
    {
        public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            var review = await unitOfWork.Repository<Review>().GetByIdAsync(request.Id);

            if (review == null) return Result.Failure("Review not found.");

            if (review.CustomerId != userId && !currentUserService.IsAdmin())
                return Result.Failure("Unauthorized.");

            unitOfWork.Repository<Review>().Delete(review);
            await unitOfWork.Complete();

            return Result.Success();
        }
    }
}