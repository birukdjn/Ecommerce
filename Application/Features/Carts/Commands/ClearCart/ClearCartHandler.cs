using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Application.Interfaces;

namespace Application.Features.Carts.Commands.ClearCart;

public class ClearCartHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<ClearCartCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId == null || userId == Guid.Empty)
            return Result<Unit>.Failure("Unauthorized");

        var cart = await unitOfWork.Repository<Cart>().Query()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null || !cart.Items.Any())
        {
            return Result<Unit>.Success(Unit.Value);
        }

        foreach (var item in cart.Items.ToList())
        {
            unitOfWork.Repository<CartItem>().Delete(item);
        }

        var result = await unitOfWork.Complete();

        return result > 0
            ? Result<Unit>.Success(Unit.Value)
            : Result<Unit>.Failure("Failed to clear the cart.");
    }
}