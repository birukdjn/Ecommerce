namespace Application.DTOs.Review
{
    public record ReviewDto(
        Guid Id,
        string CustomerName,
        string? ProfilePictureUrl,
        int Rating,
        string? Comment,
        DateTime CreatedAt);
}