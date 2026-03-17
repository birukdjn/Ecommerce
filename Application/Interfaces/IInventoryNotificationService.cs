namespace Application.Interfaces
{
    public interface IInventoryNotificationService
    {
        Task CheckLowStockAndNotifyVendors();
    }
}