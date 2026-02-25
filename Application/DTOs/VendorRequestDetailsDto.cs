namespace Application.DTOs
{
    public record VendorRequestDetailsDto(
    Guid Id,
    string StoreName,
    string? Description,
    string? LicenseUrl,
    string? LogoUrl,
    string? RejectionReason,
    string UserName,
    string UserEmail,
    string Status,
    DateTime CreatedAt);
}
