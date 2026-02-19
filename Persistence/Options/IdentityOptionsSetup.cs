using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Persistence.Options
{
    internal class IdentityOptionsSetup: IConfigureOptions<IdentityOptions>
    {
        public void Configure(IdentityOptions options)
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;

            options.User.RequireUniqueEmail = true;
        }
    }
}
