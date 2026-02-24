using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class SubOrderConfiguration : IEntityTypeConfiguration<SubOrder>
    {
        public void Configure(EntityTypeBuilder<SubOrder> builder)
        {
            builder.HasOne(s => s.MasterOrder)
                   .WithMany(o => o.SubOrders)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(so => so.VendorId);

            builder.HasIndex(so => so.OrderId);

            builder.Property(so => so.Status)
                   .HasConversion<string>();

        }
    }
}
