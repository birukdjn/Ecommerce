namespace Application.Interfaces
{
    public interface IInventoryNotificationService
    {
        Task CheckLowStockAndNotifyVendors();
        Task ReleaseExpiredUnpaidOrders();
    }
}