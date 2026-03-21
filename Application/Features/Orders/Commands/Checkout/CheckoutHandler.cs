using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands.Checkout
{
    public class CheckoutHandler(
        IUnitOfWork unitOfWork,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
        : IRequestHandler<CheckoutCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CheckoutCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("Unauthorized");

            // 1. Start an explicit Transaction to ensure "All or Nothing"
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

                if (user == null) return Result<Guid>.Failure("User not found");

                // Get Selected Cart Items with necessary Includes for Commission logic
                var cart = await unitOfWork.Repository<Cart>().Query()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.ProductCategories)
                                .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                var selectedItems = cart?.Items.Where(x => x.IsSelected).ToList();
                if (selectedItems == null || !selectedItems.Any())
                    return Result<Guid>.Failure("Cart is empty or no items selected.");

                // 2. Validate Stock & Prepare Address
                foreach (var item in selectedItems)
                {
                    if (item.Product.StockQuantity < item.Quantity)
                        return Result<Guid>.Failure($"Stock for {item.Product.Name} is insufficient.");
                }

                var shippingInfo = await GetShippingDetails(command, userId.Value, user);
                if (!shippingInfo.IsSuccess) return Result<Guid>.Failure(shippingInfo.Error!);

                var addr = shippingInfo.Value!;

                // 3. Create Master Order with a safer Unique Number
                var order = new Order
                {
                    CustomerId = userId.Value,
                    OrderNumber = $"ORD-{userId.Value.ToString()[..4].ToUpper()}-{DateTime.UtcNow.Ticks}",
                    TotalAmount = selectedItems.Sum(x => x.Quantity * x.UnitPrice),
                    Status = OrderStatus.pending,
                    PaymentStatus = PaymentStatus.Pending,
                    ShippingFullName = addr.FullName,
                    ShippingPhoneNumber = addr.Phone,
                    ShippingCountry = addr.Country,
                    ShippingRegion = addr.Region,
                    ShippingCity = addr.City,
                    ShippingSpecialPlaceName = addr.SpecialPlace
                };

                // 4. Split into SubOrders by Vendor
                var itemsByVendor = selectedItems.GroupBy(x => x.Product.VendorId);

                foreach (var vendorGroup in itemsByVendor)
                {
                    var vendorItems = vendorGroup.ToList();
                    decimal totalCommission = 0;
                    var orderItems = new List<OrderItem>();

                    foreach (var item in vendorItems)
                    {
                        // Calculate Commission based on Category
                        var commPercent = item.Product.GetCommissionPercentage();
                        totalCommission += item.Quantity * item.UnitPrice * (commPercent / 100);

                        orderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });

                        // Deduct Stock immediately (Protected by Transaction + RowVersion)
                        item.Product.StockQuantity -= item.Quantity;
                    }

                    order.SubOrders.Add(new SubOrder
                    {
                        VendorId = vendorGroup.Key,
                        SubTotal = vendorItems.Sum(i => i.Quantity * i.UnitPrice),
                        PlatformCommission = totalCommission,
                        Status = SubOrderStatus.Pending,
                        Items = orderItems
                    });
                }

                // 5. Save everything and clear cart
                unitOfWork.Repository<Order>().Add(order);
                foreach (var item in selectedItems) unitOfWork.Repository<CartItem>().Delete(item);

                await unitOfWork.Complete();

                // 6. Commit the transaction only if SaveChangesAsync succeeded
                await transaction.CommitAsync(cancellationToken);

                return Result<Guid>.Success(order.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                // If another user bought the item at the exact same time
                await transaction.RollbackAsync(cancellationToken);
                return Result<Guid>.Failure("Someone else just purchased an item in your cart. Please refresh and try again.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<Guid>.Failure("An unexpected error occurred during checkout.");
            }
        }

        private async Task<Result<ShippingInfo>> GetShippingDetails(CheckoutCommand cmd, Guid userId, ApplicationUser user)
        {
            if (cmd.AddressId.HasValue)
            {
                var address = await unitOfWork.Repository<Address>().Query()
                    .FirstOrDefaultAsync(a => a.Id == cmd.AddressId && a.UserId == userId);

                if (address == null) return Result<ShippingInfo>.Failure("Saved address not found.");

                return Result<ShippingInfo>.Success(new ShippingInfo(
                    user.FullName ?? "Customer", user.PhoneNumber ?? "", address.Country, address.Region, address.City, address.SpecialPlaceName));
            }

            if (string.IsNullOrEmpty(cmd.ShippingCountry))
                return Result<ShippingInfo>.Failure("Shipping details required.");

            return Result<ShippingInfo>.Success(new ShippingInfo(
                cmd.ShippingFullName!, cmd.ShippingPhoneNumber!, cmd.ShippingCountry, cmd.ShippingRegion!, cmd.ShippingCity!, cmd.ShippingSpecialPlaceName!));
        }

        private record ShippingInfo(string FullName, string Phone, string Country, string Region, string City, string SpecialPlace);
    }
}