namespace Application.DTOs.Admin
{
    public record UserSummaryDto
    (
        Guid Id,
        string FullName,
        string Email,
        string PhoneNumber,
        string Status,
        DateTime CreatedAt
    );
}