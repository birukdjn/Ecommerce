using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Api.Filters;

public class HangfireAuthorizationFilter(string policyName) : IDashboardAuthorizationFilter
{
    private readonly string _policyName = policyName;

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // 🔥 Authenticate using Bearer token manually
        var authResult = httpContext.AuthenticateAsync().GetAwaiter().GetResult();

        if (!authResult.Succeeded || authResult.Principal == null)
            return false;

        httpContext.User = authResult.Principal;

        // 🔐 Apply your Admin policy
        var authService = httpContext.RequestServices
            .GetRequiredService<IAuthorizationService>();

        var result = authService
            .AuthorizeAsync(httpContext.User, null, _policyName)
            .GetAwaiter()
            .GetResult();

        return result.Succeeded;
    }
}