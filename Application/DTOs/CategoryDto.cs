namespace Application.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        public List<CategoryDto> Children { get; set; } = [];
    }
}