namespace Application.DTOs.Admin
{
    public record VendorSummaryDto
    (
        Guid Id,
        string FullName,
        string StoreName,
        string Email,
        string PhoneNumber,
        string Status,
        int ProductCount,
        DateTime CreatedAt
    );
}