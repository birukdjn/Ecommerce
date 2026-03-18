using Domain.Common;
using MediatR;

namespace Application.Features.Reviews.Commands.DeleteReview
{
    public record DeleteReviewCommand(Guid Id) : IRequest<Result>;

}