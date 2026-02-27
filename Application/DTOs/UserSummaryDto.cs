
namespace Application.DTOs
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