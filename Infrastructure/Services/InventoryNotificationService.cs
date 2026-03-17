using Application.Interfaces;
using Application.Templates.Email;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class InventoryNotificationService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender):IInventoryNotificationService
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
    }
}