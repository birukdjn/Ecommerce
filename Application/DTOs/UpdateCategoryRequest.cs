namespace Application.DTOs
{
    public record UpdateCategoryRequest
    (
        string Name,
        string? Description,
        Guid? ParentCategoryId,
        decimal CommissionPercentage
    );
}