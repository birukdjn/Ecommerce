using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Domain.Common.Interfaces;

namespace Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly Guid? _currentVendorId;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
            _currentVendorId = _currentUserService.GetCurrentVendorId();
        }
        public DbSet<Address> Addresses{ get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PayoutRequest> PayoutRequests { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductImage> ProductImages{ get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SubOrder> SubOrders { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorWallet> VendorWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            // GLOBAL SOFT DELETE FILTER
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    builder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
                }
            }

            // LOGICAL MULTI-TENANCY FILTERS
            builder.Entity<SubOrder>()
                .HasQueryFilter(so => !_currentVendorId.HasValue || so.VendorId == _currentVendorId);

            builder.Entity<Product>()
                .HasQueryFilter(p => !_currentVendorId.HasValue || p.VendorId == _currentVendorId);

            // RELATIONSHIPS & CONSTRAINTS
            builder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            builder.Entity<Category>()
                .HasMany(c => c.ChildCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vendor>()
                .HasOne(v => v.User)
                .WithOne(u => u.Vendor)
                .HasForeignKey<Vendor>(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vendor>()
                .HasOne(v => v.Wallet)
                .WithOne(w => w.Vendor)
                .HasForeignKey<VendorWallet>(w => w.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubOrder>()
                .HasOne(s => s.MasterOrder)
                .WithMany(o => o.SubOrders)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reviews)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.SubOrder)
                .WithMany(so => so.Items)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .OnDelete(DeleteBehavior.Cascade);

            // INDEXES
            builder.Entity<Address>()
                .HasIndex(a => new { a.UserId, a.IsDefault })
                .HasFilter("\"IsDefault\" = true")
                .IsUnique(); 

            builder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.Entity<PaymentTransaction>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();
         
            builder.Entity<Review>()
                .HasIndex(r => new { r.ProductId, r.CustomerId })
                .IsUnique();

            builder.Entity<Category>()
                .HasIndex(c => new { c.Name, c.ParentCategoryId })
                .IsUnique();

            builder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // PROPERTIES
            builder.Entity<ApplicationUser>()
                .Property(x => x.FullName)
                .IsRequired(false)
                .HasMaxLength(150);
           
            builder.Entity<Review>()
                .Property(r => r.Rating)
                .HasDefaultValue(5);

            // PERFORMANCE INDEXES
            builder.Entity<Product>().HasIndex(p => p.VendorId);
            builder.Entity<Product>().HasIndex(p => p.Name);
            builder.Entity<SubOrder>().HasIndex(so => so.VendorId);
            builder.Entity<SubOrder>().HasIndex(so => so.OrderId);
            builder.Entity<Order>().HasIndex(o => o.CustomerId);
            builder.Entity<OrderItem>().HasIndex(oi => oi.ProductId);
            builder.Entity<Review>().HasIndex(r => r.ProductId);

            // ENUM CONVERSIONS
            builder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            builder.Entity<Order>()
                .Property(o => o.PaymentStatus)
                .HasConversion<string>();

            builder.Entity<SubOrder>()
                .Property(so => so.Status)
                .HasConversion<string>();

            builder.Entity<Vendor>()
                .Property(v => v.Status)
                .HasConversion<string>();

            builder.Entity<PayoutRequest>()
                .Property(pr => pr.Status)
                .HasConversion<string>();

            // PROPERTY CONFIGURATIONS
            foreach (var property in builder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetColumnType("decimal(18,2)");
            }

        }

        // Helper for dynamic soft-delete filter
        private static LambdaExpression ConvertFilterExpression(Type type)
        {
            var parameter = Expression.Parameter(type, "it");
            var body = Expression.Equal(Expression.Property(parameter, nameof(BaseEntity.IsDeleted)), Expression.Constant(false));
            return Expression.Lambda(body, parameter);
        }
    }
}