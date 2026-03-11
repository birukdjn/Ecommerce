using Domain.Common;
using Domain.Common.Interfaces;
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

            string fullName, phone, country, region, city, specialPlace;

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

            if (user == null) return Result<Guid>.Failure("User not found");

            var cartRepo = unitOfWork.Repository<Cart>();
            var orderRepo = unitOfWork.Repository<Order>();
            var cartItemRepo = unitOfWork.Repository<CartItem>();
            var addressRepo = unitOfWork.Repository<Address>();

            // Get Selected Cart Items
            var cart = await cartRepo.Query()
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            var selectedItems = cart?.Items.Where(x => x.IsSelected).ToList();
            if (selectedItems == null || !selectedItems.Any())
                return Result<Guid>.Failure("Cart is empty or no items selected.");

            // Initial Validation & Stock Check
            foreach (var item in selectedItems)
            {
                if (item.Product.StockQuantity < item.Quantity)
                    return Result<Guid>.Failure($"Stock for {item.Product.Name} is insufficient.");
            }

            if (command.AddressId.HasValue)
            {
                var address = await addressRepo.Query()
                    .FirstOrDefaultAsync(a => a.Id == command.AddressId && a.UserId == userId, cancellationToken);

                if (address == null)
                    return Result<Guid>.Failure("Address not found");

                fullName = user?.FullName ?? "Unknown Customer";
                phone = user?.PhoneNumber ?? "No Phone";
                country = address.Country;
                region = address.Region;
                city = address.City;
                specialPlace = address.SpecialPlaceName;
            }
            else
            {
                if (string.IsNullOrEmpty(command.ShippingCountry))
                    return Result<Guid>.Failure("Shipping details are required for new addresses.");

                fullName = command.ShippingFullName!;
                phone = command.ShippingPhoneNumber!;
                country = command.ShippingCountry!;
                region = command.ShippingRegion!;
                city = command.ShippingCity!;
                specialPlace = command.ShippingSpecialPlaceName!;

                var newAddress = new Address
                {
                    UserId = userId.Value,
                    Country = country,
                    Region = region,
                    City = city,
                    SpecialPlaceName = specialPlace,
                    IsDefault = false
                };

                addressRepo.Add(newAddress);
            }

            // Create Master Order
            var order = new Order
            {
                CustomerId = userId.Value,
                OrderNumber = $"ORD-{DateTime.UtcNow.Ticks}",
                TotalAmount = selectedItems.Sum(x => x.Quantity * x.UnitPrice),
                Status = OrderStatus.pending,
                PaymentStatus = PaymentStatus.Pending,
                ShippingFullName = fullName,
                ShippingPhoneNumber = phone,
                ShippingCountry = country,
                ShippingRegion = region,
                ShippingCity = city,
                ShippingSpecialPlaceName = specialPlace
            };

            // Split into SubOrders by Vendor
            var itemsByVendor = selectedItems.GroupBy(x => x.Product.VendorId);

            foreach (var vendorGroup in itemsByVendor)
            {
                var vendorId = vendorGroup.Key;
                var vendorItems = vendorGroup.ToList();

                var subTotal = vendorItems.Sum(i => i.Quantity * i.UnitPrice);

                // Calculate Commission (Simplified logic)
                decimal totalCommission = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in vendorItems)
                {
                    var commPercent = item.Product.GetCommissionPercentage();
                    totalCommission += item.Quantity * item.UnitPrice * (commPercent / 100);

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });

                    // Deduct Stock
                    item.Product.StockQuantity -= item.Quantity;
                }

                var subOrder = new SubOrder
                {
                    VendorId = vendorId,
                    SubTotal = subTotal,
                    PlatformCommission = totalCommission,
                    Status = SubOrderStatus.Pending,
                    Items = orderItems
                };

                order.SubOrders.Add(subOrder);
            }

            //  Save and Clear Cart
            orderRepo.Add(order);
            foreach (var item in selectedItems) cartItemRepo.Delete(item);

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(order.Id)
                : Result<Guid>.Failure("Failed to process order.");
        }
    }
}