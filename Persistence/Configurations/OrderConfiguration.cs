using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasOne(o => o.Customer)
                   .WithMany(u => u.Orders)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.OrderNumber)
                   .IsUnique();

            builder.HasIndex(o => o.CustomerId);

            builder.Property(o => o.Status)
                   .HasConversion<string>();

            builder.Property(o => o.PaymentStatus)
                   .HasConversion<string>();

        }
    }
}
