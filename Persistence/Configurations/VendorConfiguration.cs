

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
    {
        public void Configure(EntityTypeBuilder<Vendor> builder)
        {
            builder.HasOne(v => v.User)
                   .WithOne(u => u.Vendor)
                   .HasForeignKey<Vendor>(v => v.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Wallet)
                   .WithOne(w => w.Vendor)
                   .HasForeignKey<VendorWallet>(w => w.VendorId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(v => v.Status)
                   .HasConversion<string>();


        }
    }
}
