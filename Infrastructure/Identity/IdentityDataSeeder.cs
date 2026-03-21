using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedAllAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            await SeedRolesAsync(serviceProvider);
            await SeedUsersAsync(serviceProvider, cancellationToken);
        }

        private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>() ?? throw new Exception("RoleManager not registered in DI");
            string[] roles = { Roles.Admin, Roles.Vendor, Roles.Customer };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
        }

        private static async Task SeedUsersAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>() ?? throw new Exception("UserManager not registered in DI");
            var usersToSeed = new[]
            {
                new { Email = "Birukedjn@gmail.com", Password = "Admin123!", FullName="Admin Admin", Phone="0911111111", Role=Roles.Admin, IsVendor=false, StoreName=(string?)null },
                new { Email = "birukdejene2000@gmail.com", Password = "User123!", FullName="Biruk Dejene", Phone="0908574808", Role=Roles.Customer, IsVendor=false, StoreName=(string?)null },
                new { Email = "birukdejene64@gmail.com", Password = "User123!", FullName="User2 User2", Phone="0922222222", Role=Roles.Customer, IsVendor=false, StoreName=(string?)null },
                new { Email = "birukyihun2000@gmail.com", Password = "Vendor123!", FullName="biruk yihun", Phone="0777888325", Role=Roles.Vendor, IsVendor=true, StoreName=(string?)"biruk yihun" },
                new { Email = "birukelove999@gmail.com", Password = "Vendor123!", FullName="Vendor2 Vendor2", Phone="0933333333", Role=Roles.Vendor, IsVendor=true, StoreName=(string?)"Vendor2 Vendor2" }
            };

            foreach (var userInfo in usersToSeed)
            {
                try
                {
                    await CreateUserIfNotExists(userManager, serviceProvider, userInfo.Email, userInfo.Password,
                        userInfo.FullName, userInfo.Phone, userInfo.Role, userInfo.IsVendor, userInfo.StoreName, cancellationToken);
                }
                catch (Exception ex)
                {
                    // You can replace this with proper logging
                    Console.WriteLine($"Error seeding user {userInfo.Email}: {ex.Message}");
                }
            }
        }

        private static async Task<ApplicationUser> CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            IServiceProvider serviceProvider,
            string email,
            string password,
            string fullName,
            string phoneNumber,
            string role,
            bool isVendor = false,
            string? storeName = null,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null) return user;

            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = true,
                FullName = fullName,
                IsActive = true
            };

            var result = await userManager.CreateAsync(newUser, password);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user {email}: {string.Join(",", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(newUser, role);

            if (isVendor)
            {
                var context = serviceProvider.GetRequiredService<IApplicationDbContext>();

                var vendor = new Vendor
                {
                    Id = Guid.NewGuid(),
                    UserId = newUser.Id,
                    StoreName = storeName ?? $"{fullName}'s Store",
                    Description = $"Sample store for {fullName}",
                    Status = VendorStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };
                context.Vendors.Add(vendor);

                context.VendorWallets.Add(new VendorWallet
                {
                    Id = Guid.NewGuid(),
                    VendorId = vendor.Id,
                    Balance = 0,
                    EscrowBalance = 0
                });

                await context.SaveChangesAsync(cancellationToken);
            }

            return newUser;
        }
    }
}