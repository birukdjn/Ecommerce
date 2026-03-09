using Domain.Common;

namespace Domain.Entities
{
    public class Category : AuditableEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal CommissionPercentage { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }


        public virtual ICollection<Category> ChildCategories { get; set; } = [];
        public virtual ICollection<ProductCategory> ProductCategories { get; set; } = [];

        public decimal GetEffectiveCommission()
        {
            if (CommissionPercentage > 0) return CommissionPercentage;

            return ParentCategory?.GetEffectiveCommission() ?? 0;
        }

        public List<string> GetCategoryPath()
        {
            var path = new List<string>();
            Category? current = this;
            while (current != null)
            {
                path.Insert(0, current.Name);
                current = current.ParentCategory;
            }
            return path;
        }

    }
}
