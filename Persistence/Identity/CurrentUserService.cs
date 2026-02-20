using System.Security.Claims;
using Domain.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Persistence.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid? GetCurrentUserId()
    {
        var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    public Guid? GetCurrentVendorId()
    {
        var vendorId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("VendorId");
        return Guid.TryParse(vendorId, out var guid) ? guid : null;
    }

    public bool IsAuthenticated() =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsVendor() =>
        _httpContextAccessor.HttpContext?.User?.IsInRole("Vendor") ?? false;

    public bool IsAdmin() =>
        _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
}