namespace Application.DTOs.Address
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Country { get; set; } = default!;
        public string Region { get; set; } = default!;
        public string City { get; set; } = default!;
        public string? SpecialPlaceName { get; set; }
        public bool IsDefault { get; set; }
    }
}