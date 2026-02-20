using Domain.Common;

namespace Domain.Entities
{
    public class Category:BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal CommissionPercentage { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }


        public virtual ICollection<Category> ChildCategories { get; set; } = [];
        public virtual ICollection<ProductCategory> ProductCategories { get; set; } = [];


    }
}
