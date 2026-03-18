namespace Application.DTOs.Review
{
    public record UpdateReviewDto
    (
        int? Rating,
        string? Comment
    );
}