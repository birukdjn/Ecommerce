using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Domain.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; }
        DbSet<Vendor> Vendors { get; set; }
        DbSet<VendorWallet> VendorWallets { get; set; }
        DbSet<Address> Addresses { get; set; }
        DbSet<Cart> Carts { get; set; }
        DbSet<CartItem> CartItems { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Order> Orders { get; set; }
        DbSet<OrderItem> OrderItems { get; set; }
        DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        DbSet<PayoutRequest> PayoutRequests { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<ProductCategory> ProductCategories { get; set; }
        DbSet<ProductImage> ProductImages { get; set; }
        DbSet<Review> Reviews { get; set; }
        DbSet<SubOrder> SubOrders { get; set; }
        DbSet<WalletTransaction> WalletTransactions { get; set; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DatabaseFacade Database { get; }



    }
}
