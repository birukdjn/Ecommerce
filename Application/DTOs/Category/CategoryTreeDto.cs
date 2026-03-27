namespace Application.DTOs.Category
{
    public class CategoryTreeDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public List<CategoryTreeDto> Children { get; set; } = [];
    }
}