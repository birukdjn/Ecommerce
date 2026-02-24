using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasOne(r => r.Customer)
                   .WithMany(u => u.Reviews)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.ProductId, r.CustomerId })
                   .IsUnique();

            builder.Property(r => r.Rating)
                   .HasDefaultValue(5);

        }
    }
}
