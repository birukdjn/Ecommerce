using Application.DTOs.Cart;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Commands.ValidateCart
{
    public class ValidateCartHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<ValidateCartCommand, Result<CartValidationDto>>
    {
        public async Task<Result<CartValidationDto>> Handle(ValidateCartCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            var items = await unitOfWork.Repository<CartItem>().Query()
                .Include(i => i.Product)
                .Where(i => i.Cart.UserId == userId && i.IsSelected)
                .ToListAsync(cancellationToken);

            var errors = new List<string>();

            foreach (var item in items)
            {
                if (item.Product.IsDeleted || !item.Product.IsApproved)
                    errors.Add($"Product '{item.Product.Name}' is no longer available.");

                else if (item.Product.StockQuantity < item.Quantity)
                    errors.Add($"Only {item.Product.StockQuantity} units of '{item.Product.Name}' are left.");
            }

            return Result<CartValidationDto>.Success(new CartValidationDto(errors.Count == 0, errors));
        }
    }
}