namespace Api.Controllers.Requests
{
    public class BecomeVendorRequest
    {
        public string StoreName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public IFormFile LogoFile { get; set; } = null!;
        public IFormFile LicenseFile { get; set; } = null!;
    }
}