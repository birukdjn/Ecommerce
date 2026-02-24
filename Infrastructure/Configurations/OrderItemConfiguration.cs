using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {

            builder.HasOne(oi => oi.Product)
                   .WithMany(p => p.OrderItems)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(oi => oi.SubOrder)
                   .WithMany(so => so.Items)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(oi => oi.ProductId);


        }
    }
}
