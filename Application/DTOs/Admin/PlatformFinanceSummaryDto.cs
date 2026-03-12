namespace Application.DTOs.Admin
{
    public record PlatformFinanceSummaryDto
    (

        decimal TotalSales,
        decimal TotalCommissionEarned,
        decimal ActiveEscrowBalance,
        decimal TotalPayoutsProcessed
    );
}