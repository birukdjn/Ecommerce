using Application.DTOs.Cart;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Queries.GetCart
{
    public class GetCartHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<GetCartQuery, Result<CartDto>>
    {
        public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<CartDto>.Failure("Unauthorized");

            var cart = await unitOfWork.Repository<Cart>().Query()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart == null)
            {
                return Result<CartDto>.Success(new CartDto { Id = Guid.Empty });
            }

            var cartDto = new CartDto
            {
                Id = cart.Id,
                Items = cart.Items.Select(item => new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductImage = item.Product.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                                   ?? item.Product.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                }).ToList()
            };

            return Result<CartDto>.Success(cartDto);
        }
    }
}