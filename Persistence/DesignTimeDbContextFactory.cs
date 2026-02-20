using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Persistence.Context;
using Domain.Common.Interfaces;

namespace Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();


        optionsBuilder.UseNpgsql("Host=localhost;Database=ecommerce;Username=Birukdjn;Password=Birukdjn@8325");

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeUserService());
    }
}

public class DesignTimeUserService : ICurrentUserService
{
    public Guid? GetCurrentUserId() => Guid.Empty;
    public Guid? GetCurrentVendorId() => null;
    public bool IsAuthenticated() => false;
    public bool IsVendor() => false;
    public bool IsAdmin() => false;
}