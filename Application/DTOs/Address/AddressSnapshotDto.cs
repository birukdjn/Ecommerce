namespace Application.DTOs.Address
{
    public record AddressSnapshotDto(
        string FullName,
        string Phone,
        string Country,
        string Region,
        string City,
        string SpecialPlaceName
    );
}