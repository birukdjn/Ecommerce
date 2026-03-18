using Domain.Common;
using MediatR;

namespace Application.Features.Reviews.Commands.UpdateReview
{
    public record UpdateReviewCommand(Guid ReviewId, int? Rating, string? Comment) : IRequest<Result>;
}