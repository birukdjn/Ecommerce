
namespace Application.DTOs
{
    public record AdminStatsDto(
        int TotalUsers,
        int TotalActiveUsers,
        int TotalVendors,
        int PendingVendors,
        int ActiveVendors,
        int TotalOrders,
        int TotalProducts,
        int TotalCategories
    );
}