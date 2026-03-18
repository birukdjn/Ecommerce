using Domain.Common;
using MediatR;

namespace Application.Features.Reviews.Commands.CreateReview
{
    public record CreateReviewCommand(Guid ProductId, int Rating, string? Comment) : IRequest<Result<Guid>>;

}