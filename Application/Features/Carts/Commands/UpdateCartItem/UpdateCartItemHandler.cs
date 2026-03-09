using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Commands.UpdateCartItem
{

    public class UpdateCartItemHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<UpdateCartItemCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<Unit>.Failure("Unauthorized");

            var cartItem = await unitOfWork.Repository<CartItem>().Query()
                .Include(i => i.Cart)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == request.CartItemId && i.Cart.UserId == userId, cancellationToken);

            if (cartItem == null)
                return Result<Unit>.Failure("Cart item not found.");

            if (request.Quantity <= 0)
                return Result<Unit>.Failure("Quantity must be at least 1. ");

            if (cartItem.Product.StockQuantity < request.Quantity)
                return Result<Unit>.Failure($"Insufficient stock. Only {cartItem.Product.StockQuantity} available.");

            cartItem.Quantity = request.Quantity;
            cartItem.UnitPrice = cartItem.Product.Price;

            unitOfWork.Repository<CartItem>().Update(cartItem);

            var result = await unitOfWork.Complete();
            return result > 0
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure("Failed to update cart item.");
        }
    }
}