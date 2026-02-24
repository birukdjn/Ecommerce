using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq.Expressions;

namespace Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
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

        // Identity override
        public override DbSet<ApplicationUser> Users => base.Users;

        // Custom DbSets
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PayoutRequest> PayoutRequests { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SubOrder> SubOrders { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorWallet> VendorWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        public new DatabaseFacade Database => base.Database;
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {


            base.OnModelCreating(builder);

            var assembly = typeof(ApplicationDbContext).Assembly;

            builder.ApplyConfigurationsFromAssembly(assembly);

            foreach (var property in builder.Model.GetEntityTypes()
                 .SelectMany(t => t.GetProperties())
                 .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    builder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.RowVersion))
                        .HasColumnName("xmin") 
                        .HasColumnType("xid")  
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                }
            }

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