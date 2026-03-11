namespace Application.DTOs.Admin
{
    public record PayoutRequestDto(
        Guid Id,
        Guid VendorId,
        string StoreName,
        decimal Amount,
        string Status,
        DateTime CreatedAt
    );
}