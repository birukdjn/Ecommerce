using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Commands.ToggleCartItemSelection
{


    public class ToggleSelectionHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<ToggleSelectionCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(ToggleSelectionCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<Unit>.Failure("Unauthorized");
                
            var item = await unitOfWork.Repository<CartItem>().Query()
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == command.CartItemId && i.Cart.UserId == userId, cancellationToken);

            if (item == null) return Result<Unit>.Failure("Item not found");

            item.IsSelected = command.IsSelected;
            unitOfWork.Repository<CartItem>().Update(item);
            await unitOfWork.Complete();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}