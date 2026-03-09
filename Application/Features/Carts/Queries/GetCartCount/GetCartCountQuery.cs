using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Queries.GetCartCount
{

    public class GetCartCountHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<GetCartCountQuery, Result<int>>
    {
        public async Task<Result<int>> Handle(GetCartCountQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<int>.Failure("Unauthorized");

            var count = await unitOfWork.Repository<CartItem>().Query()
                .Where(i => i.Cart.UserId == userId)
                .SumAsync(i => i.Quantity, cancellationToken);

            return Result<int>.Success(count);
        }
    }
}