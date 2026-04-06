using Application.Interfaces;
using Application.Templates.Email;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class InventoryNotificationService(
        IUnitOfWork unitOfWork,
        IJobService jobService,
        IEmailSender emailSender) : IInventoryNotificationService
    {
        public async Task CheckLowStockAndNotifyVendors()
        {
            var threshold = 5;

            var lowStockProducts = await unitOfWork.Repository<Product>().Query()
                .Include(p => p.Vendor)
                    .ThenInclude(v => v.User)
                .Where(p => p.Status == ProductStatus.Approved &&
                            p.StockQuantity <= threshold &&
                            !p.IsDeleted)
                .ToListAsync();

            foreach (var product in lowStockProducts)
            {
                if (product.Vendor?.User != null && !string.IsNullOrEmpty(product.Vendor.User.Email))
                {
                    await emailSender.SendEmailAsync(
                        product.Vendor.User.Email,
                        $"Low Stock Alert: {product.Name}",
                        EmailTemplates.GetLowStockEmail(
                            product.Vendor.User.FullName ?? product.Vendor.StoreName,
                            product.Name,
                            product.StockQuantity)
                    );
                }
            }
        }

        public async Task ReleaseExpiredUnpaidOrders()
        {
            var expirationTime = DateTime.UtcNow.AddMinutes(-15);

            var expiredOrders = await unitOfWork.Repository<Order>().Query()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.PaymentStatus == PaymentStatus.Pending &&
                            o.CreatedAt <= expirationTime &&
                            o.Status == OrderStatus.Pending)
                .ToListAsync();

            if (!expiredOrders.Any()) return;

            foreach (var order in expiredOrders)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = item.Product;
                    product.StockQuantity += item.Quantity;
                    unitOfWork.Repository<Product>().Update(product);
                }

                order.Status = OrderStatus.Cancelled;
                unitOfWork.Repository<Order>().Update(order);



                jobService.Enqueue<IEmailSender>(s => s.SendEmailAsync(
                    order.Customer.Email!,
                    $"Order #{order.OrderNumber} Expired",
                    EmailTemplates.GetOrderExpiredEmail(order.Customer.FullName!, order.OrderNumber)
                ));
            }

            await unitOfWork.Complete();
        }
    }
}