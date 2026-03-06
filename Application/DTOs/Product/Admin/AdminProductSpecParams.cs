namespace Application.DTOs.Product.Admin
{
    public class AdminProductSpecParams : ProductSpecParams
    {
        public bool? IsApproved { get; set; }
        public bool? IsDeleted { get; set; }
        public Guid? VendorId { get; set; }
        public Guid? CategoryId { get; set; }
    }
}