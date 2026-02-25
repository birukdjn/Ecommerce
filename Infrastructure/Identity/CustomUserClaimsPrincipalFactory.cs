using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Infrastructure.Identity
{

    public class CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> optionsAccessor) : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>(userManager, roleManager, optionsAccessor)
    {
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var userWithVendor = await UserManager.Users
                .Include(u => u.Vendor)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userWithVendor?.Vendor != null)
            {
                identity.AddClaim(new Claim("VendorId", userWithVendor.Vendor.Id.ToString()));
                identity.AddClaim(new Claim("IsActive", userWithVendor.IsActive.ToString().ToLower()));
            }

            return identity;
        }
    }
}