using Domain.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedAllAsync(IServiceProvider serviceProvider)
        {
            await SeedRolesAsync(serviceProvider);

            await SeedAdminUserAsync(serviceProvider);
        }

        private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            string[] roles = [Roles.Admin, Roles.Vendor, Roles.Customer];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
        }
        private static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var adminEmail = "admin@gmail.com";
            var userEmail = "user@gmail.com";
            var vendorEmail = "vendor@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            var normalUser = await userManager.FindByEmailAsync(userEmail);
            var vendorUser = await userManager.FindByEmailAsync(vendorEmail);


            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0777888325",
                    PhoneNumberConfirmed = true,
                    FullName = "Admin Admin",
                    IsActive = true
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, Roles.Admin);
                }
            }
            if (normalUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0908574808",
                    PhoneNumberConfirmed = true,
                    FullName = "user user",
                    IsActive = true
                };

                var result = await userManager.CreateAsync(newUser, "User123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, Roles.Customer);
                }
            }
            if (vendorUser == null)
            {
                var newVendor = new ApplicationUser
                {
                    UserName = vendorEmail,
                    Email = vendorEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0912345678",
                    PhoneNumberConfirmed = true,
                    FullName = "vendor vendor",
                    IsActive = true
                };

                var result = await userManager.CreateAsync(newVendor, "Vendor123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newVendor, Roles.Vendor);
                    var context = serviceProvider.GetRequiredService<IApplicationDbContext>();
                    var vendor = new Vendor
                    {
                        Id = Guid.NewGuid(),
                        UserId = newVendor.Id,
                        StoreName = "Vendor Store",
                        Description = "This is a sample vendor created during seeding.",
                        Status = VendorStatus.Active,
                        CreatedAt = DateTime.UtcNow,

                    };
                    context.Vendors.Add(vendor);

                    var wallet = new VendorWallet
                    {
                        Id = Guid.NewGuid(),
                        VendorId = vendor.Id,
                        Balance = 0,
                        EscrowBalance = 0
                    };
                    context.VendorWallets.Add(wallet);



                    await context.SaveChangesAsync(CancellationToken.None);
                }
            }
        }
    }
}