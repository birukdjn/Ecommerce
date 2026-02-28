using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.FullName)
                   .IsRequired(false)
                   .HasMaxLength(150);

            builder.HasIndex(u => u.PhoneNumber).IsUnique();

            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}
