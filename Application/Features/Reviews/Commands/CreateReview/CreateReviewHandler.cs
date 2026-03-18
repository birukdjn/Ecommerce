using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<CreateReviewCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateReviewCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();

            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("User must be authenticated to leave a review.");

            var hasPurchased = await unitOfWork.Repository<OrderItem>().Query()
                .AnyAsync(oi => oi.ProductId == command.ProductId &&
                               oi.SubOrder.MasterOrder.CustomerId == userId.Value &&
                               oi.SubOrder.Status == SubOrderStatus.Delivered, cancellationToken);

            if (!hasPurchased)
                return Result<Guid>.Failure("You can only review products you have received.");

            var existing = await unitOfWork.Repository<Review>().Query()
                .AnyAsync(r => r.ProductId == command.ProductId && r.CustomerId == userId.Value, cancellationToken);

            if (existing)
                return Result<Guid>.Failure("You have already reviewed this product.");

            var review = new Review
            {
                ProductId = command.ProductId,
                CustomerId = userId.Value,
                Rating = command.Rating,
                Comment = command.Comment,
                IsApproved = true
            };

            unitOfWork.Repository<Review>().Add(review);
            await unitOfWork.Complete();

            return Result<Guid>.Success(review.Id);
        }
    }
}