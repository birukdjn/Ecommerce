using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Commands.AddToCart;

public class AddToCartHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<AddToCartCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddToCartCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId == null || userId == Guid.Empty)
            return Result<Guid>.Failure("Unauthorized");

        var cartRepo = unitOfWork.Repository<Cart>();
        var cart = await cartRepo.Query()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart { UserId = userId.Value };
            cartRepo.Add(cart);

        }

        var product = await unitOfWork.Repository<Product>().GetByIdAsync(command.ProductId);
        if (product == null) return Result<Guid>.Failure("Product not found.");

        if (command.Quantity <= 0)
            return Result<Guid>.Failure("Quantity must be at least 1.");

        if (product.StockQuantity < command.Quantity)
            return Result<Guid>.Failure($"Insufficient stock. Only {product.StockQuantity} available.");

        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == command.ProductId);

        if (existingItem != null)
        {
            if (product.StockQuantity < (existingItem.Quantity + command.Quantity))
                return Result<Guid>.Failure("Total quantity in cart exceeds available stock.");

            existingItem.Quantity += command.Quantity;
            existingItem.UnitPrice = product.Price;
            unitOfWork.Repository<CartItem>().Update(existingItem);
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = command.ProductId,
                Quantity = command.Quantity,
                UnitPrice = product.Price
            };
            unitOfWork.Repository<CartItem>().Add(newItem);
        }

        var result = await unitOfWork.Complete();
        return result > 0
            ? Result<Guid>.Success(cart.Id)
            : Result<Guid>.Failure("Failed to update cart.");
    }
}