using Application.DTOs.Product;
using Domain.Enums;

namespace Application.DTOs.Admin
{
    public class AdminProductSpecParams : ProductSpecParams
    {
        public bool? IsDeleted { get; set; }
        public ProductStatus? ProductStatus { get; set; }
        public Guid? VendorId { get; set; }
        public Guid? CategoryId { get; set; }
    }
}