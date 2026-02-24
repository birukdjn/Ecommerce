using Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Extensions
{
    public static class IdentityResultExtensions
    {
        public static Result ToResult(this IdentityResult result)
        {
            if (result.Succeeded) return Result.Success();

            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(error);
        }
    }
}
