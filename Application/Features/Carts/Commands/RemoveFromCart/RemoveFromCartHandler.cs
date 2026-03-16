using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Application.Interfaces;

namespace Application.Features.Carts.Commands.RemoveFromCart;

public class RemoveFromCartHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<RemoveFromCartCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var cartItem = await unitOfWork.Repository<CartItem>().Query()
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == request.CartItemId && i.Cart.UserId == userId, cancellationToken);

        if (cartItem == null) return Result<Unit>.Failure("Item not found in your cart.");

        unitOfWork.Repository<CartItem>().Delete(cartItem);

        var result = await unitOfWork.Complete();
        return result > 0 ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Failed to remove item.");
    }
}