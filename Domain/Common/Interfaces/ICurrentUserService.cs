
namespace Domain.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? GetCurrentUserId();
        Guid? GetCurrentVendorId();
        bool IsAuthenticated();
        bool IsVendor();
        bool IsAdmin();
    }
}
