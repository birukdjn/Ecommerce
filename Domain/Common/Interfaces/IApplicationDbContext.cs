using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Domain.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; }
        DbSet<Vendor> Vendors { get; }
        DbSet<VendorWallet> VendorWallets { get; }
        DbSet<Address> Addresses { get;}
        DbSet<Cart> Carts { get; }
        DbSet<CartItem> CartItems { get; }
        DbSet<Category> Categories { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderItem> OrderItems { get; }
        DbSet<PaymentTransaction> PaymentTransactions { get; }
        DbSet<PayoutRequest> PayoutRequests { get; }
        DbSet<Product> Products { get; }
        DbSet<ProductCategory> ProductCategories { get; }
        DbSet<ProductImage> ProductImages { get; }
        DbSet<Review> Reviews { get; }
        DbSet<SubOrder> SubOrders { get; }
        DbSet<WalletTransaction> WalletTransactions { get; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DatabaseFacade Database { get; }



    }
}
