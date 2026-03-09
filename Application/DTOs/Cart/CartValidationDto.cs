namespace Application.DTOs.Cart
{
    public record CartValidationDto(bool IsValid, List<string> Errors);
}