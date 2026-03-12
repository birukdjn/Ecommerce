namespace Application.DTOs.Admin
{
    public record UpdateCategoryRequest
    (
        string Name,
        string? Description,
        Guid? ParentCategoryId,
        decimal CommissionPercentage
    );
}